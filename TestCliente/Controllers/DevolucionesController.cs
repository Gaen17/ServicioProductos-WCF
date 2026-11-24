using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TestCliente.Models.Devoluciones;
using TestCliente.Servicios;

namespace TestCliente.Controllers
{
    /// <summary>
    /// Sales returns screens. Validates user input here; the service performs the final
    /// quantity validation and its message is shown as is.
    /// View messages: TempData["Exito"], ViewBag.Errores (input), ViewBag.ErrorServicio, ViewBag.Info.
    /// </summary>
    public class DevolucionesController : Controller
    {
        private readonly IDevolucionesServicio _servicio;

        public DevolucionesController() : this(new DevolucionesServicio()) { }

        public DevolucionesController(IDevolucionesServicio servicio)
        {
            _servicio = servicio;
        }

        // GET /Devoluciones
        public ActionResult Index()
        {
            var lista = Invocar(() => _servicio.ListarDevoluciones());
            return View(lista ?? new List<DevolucionResumenViewModel>());
        }

        // GET /Devoluciones/Detalle/5
        public ActionResult Detalle(int? id)
        {
            var modelo = new DevolucionDetalleViewModel { IdDevolucion = id ?? 0 };
            if (id == null || id <= 0)
            {
                ViewBag.Errores = new List<string> { "El número de devolución no es válido." };
                return View(modelo);
            }

            modelo.Items = Invocar(() => _servicio.VerDevolucionDetalle(id.Value)) ?? new List<DevolucionDetalleItemViewModel>();
            return View(modelo);
        }

        // GET /Devoluciones/Nueva
        public ActionResult Nueva()
        {
            return View(new NuevaDevolucionViewModel());
        }

        // POST /Devoluciones/Nueva  (accion = buscar | agregar | confirmar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Nueva(NuevaDevolucionViewModel modelo, string accion)
        {
            // The view model is rebuilt here; clear ModelState so the view shows these values.
            ModelState.Clear();
            modelo.Productos = modelo.Productos ?? new List<ProductoVentaViewModel>();
            modelo.Detalle = modelo.Detalle ?? new List<ItemDetalleViewModel>();

            switch (accion)
            {
                case "buscar":
                    return BuscarProductos(modelo);
                case "agregar":
                    return AgregarAlDetalle(modelo);
                case "confirmar":
                    return Confirmar(modelo);
                default:
                    return View(modelo);
            }
        }

        private ActionResult BuscarProductos(NuevaDevolucionViewModel modelo)
        {
            int idVenta;
            var errores = new List<string>();
            if (!ValidarIdVenta(modelo.IdVenta, errores, out idVenta)) return ConErrores(modelo, errores);

            modelo.Productos = new List<ProductoVentaViewModel>();
            modelo.Detalle = new List<ItemDetalleViewModel>();
            modelo.IdVentaBuscada = null;

            var productos = Invocar(() => _servicio.ObtenerProductosVenta(idVenta));
            if (productos != null)
            {
                modelo.Productos = productos;
                modelo.IdVentaBuscada = idVenta;
                if (productos.Count == 0) ViewBag.Info = "La venta no tiene productos para devolver.";
            }
            return View("Nueva", modelo);
        }

        private ActionResult AgregarAlDetalle(NuevaDevolucionViewModel modelo)
        {
            var errores = new List<string>();
            int idVenta;
            if (!ValidarIdVenta(modelo.IdVenta, errores, out idVenta)) return ConErrores(modelo, errores);
            if (!ValidarVentaBuscada(modelo, idVenta, errores)) return ConErrores(modelo, errores);

            var seleccionados = modelo.Productos.Where(p => p.Seleccionado).ToList();
            if (seleccionados.Count == 0)
            {
                errores.Add("Seleccione al menos un producto.");
                return ConErrores(modelo, errores);
            }

            // Validate everything first; add nothing if any line is wrong.
            var nuevos = new List<ItemDetalleViewModel>();
            foreach (var p in seleccionados)
            {
                int cantidad;
                if (!int.TryParse((p.CantidadADevolver ?? "").Trim(), out cantidad))
                {
                    errores.Add("La cantidad a devolver de \"" + p.NombreProducto + "\" debe ser un número entero.");
                    continue;
                }
                if (cantidad <= 0)
                {
                    errores.Add("La cantidad a devolver de \"" + p.NombreProducto + "\" debe ser mayor que cero.");
                    continue;
                }

                var yaEnDetalle = modelo.Detalle.Where(d => d.IdProducto == p.IdProducto).Sum(d => d.Cantidad);
                if (cantidad + yaEnDetalle > p.CantidadPendiente)
                {
                    errores.Add("La cantidad a devolver de \"" + p.NombreProducto + "\" (" + (cantidad + yaEnDetalle) +
                                ") supera la cantidad pendiente (" + p.CantidadPendiente + ").");
                    continue;
                }

                nuevos.Add(new ItemDetalleViewModel
                {
                    IdProducto = p.IdProducto,
                    NombreProducto = p.NombreProducto,
                    Cantidad = cantidad,
                    PrecioVenta = p.PrecioVenta,
                    ReingresaInventario = p.ReingresaInventario
                });
            }
            if (errores.Count > 0) return ConErrores(modelo, errores);

            foreach (var item in nuevos)
            {
                var existente = modelo.Detalle.FirstOrDefault(d => d.IdProducto == item.IdProducto);
                if (existente == null) modelo.Detalle.Add(item);
                else
                {
                    existente.Cantidad += item.Cantidad;
                    existente.ReingresaInventario = item.ReingresaInventario;
                }
            }

            foreach (var p in modelo.Productos)
            {
                p.Seleccionado = false;
                p.CantidadADevolver = null;
            }
            return View("Nueva", modelo);
        }

        private ActionResult Confirmar(NuevaDevolucionViewModel modelo)
        {
            var errores = new List<string>();
            int idVenta;
            ValidarIdVenta(modelo.IdVenta, errores, out idVenta);
            if (errores.Count == 0) ValidarVentaBuscada(modelo, idVenta, errores);
            if (string.IsNullOrWhiteSpace(modelo.Motivo)) errores.Add("Ingrese el motivo de la devolución.");
            if (modelo.Detalle.Count == 0) errores.Add("Agregue al menos un producto al detalle antes de confirmar.");
            if (errores.Count > 0) return ConErrores(modelo, errores);

            var resultado = Invocar(() => _servicio.RegistrarDevolucion(idVenta, modelo.Motivo.Trim(), modelo.Detalle));
            if (resultado == null) return View("Nueva", modelo);

            if (!resultado.Exito)
            {
                // Final validation belongs to the service: show its message as is.
                ViewBag.ErrorServicio = resultado.Mensaje;
                return View("Nueva", modelo);
            }

            TempData["Exito"] = resultado.Mensaje + " (N° " + resultado.IdDevolucion + ", tipo: " +
                                resultado.TipoDevolucion + ", monto devuelto: " + resultado.MontoTotalDevuelto.ToString("N2") + ")";
            return RedirectToAction("Index");
        }

        private static bool ValidarIdVenta(string texto, List<string> errores, out int idVenta)
        {
            idVenta = 0;
            if (string.IsNullOrWhiteSpace(texto))
            {
                errores.Add("Ingrese el número de venta.");
                return false;
            }
            if (!int.TryParse(texto.Trim(), out idVenta) || idVenta <= 0)
            {
                errores.Add("El número de venta debe ser un entero mayor que cero.");
                return false;
            }
            return true;
        }

        private static bool ValidarVentaBuscada(NuevaDevolucionViewModel modelo, int idVenta, List<string> errores)
        {
            if (modelo.IdVentaBuscada == idVenta) return true;
            errores.Add("Presione \"Buscar productos\" para cargar los productos de la venta " + idVenta + ".");
            return false;
        }

        private ActionResult ConErrores(NuevaDevolucionViewModel modelo, List<string> errores)
        {
            ViewBag.Errores = errores;
            return View("Nueva", modelo);
        }

        /// <summary>
        /// Calls the service and turns its exceptions into view messages. Returns default(T) on failure.
        /// </summary>
        private T Invocar<T>(Func<T> llamada) where T : class
        {
            try
            {
                return llamada();
            }
            catch (OperacionNoDisponibleException ex)
            {
                ViewBag.Info = ex.Message;
            }
            catch (ErrorServicioException ex)
            {
                ViewBag.ErrorServicio = ex.Message;
            }
            return null;
        }
    }
}
