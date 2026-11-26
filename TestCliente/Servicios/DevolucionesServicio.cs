using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using TestCliente.Models.Devoluciones;
using TestCliente.Proxy;
using TestCliente.ServicioProductosRef;

namespace TestCliente.Servicios
{
    /// <summary>
    /// Implementation over the WCF proxy (HTTPS + Basic, via ProductoServiceFactory).
    /// </summary>
    public class DevolucionesServicio : IDevolucionesServicio
    {
        public List<DevolucionResumenViewModel> ListarDevoluciones()
        {
            var r = Ejecutar(c => c.ListarDevoluciones());
            ValidarExito(r.Exito, r.Mensaje);
            return (r.Devoluciones ?? new List<DevolucionListadoDto>()).Select(d => new DevolucionResumenViewModel
            {
                Id = d.Id,
                Fecha = d.Fecha,
                IdVenta = d.IdVenta,
                MotivoDevolucion = d.MotivoDevolucion,
                TipoDevolucion = d.TipoDevolucion,
                MontoTotalDevuelto = d.MontoTotalDevuelto
            }).ToList();
        }

        public List<DevolucionDetalleItemViewModel> VerDevolucionDetalle(int idDevolucion)
        {
            var r = Ejecutar(c => c.VerDevolucionDetalle(idDevolucion));
            ValidarExito(r.Exito, r.Mensaje);
            return (r.Detalle ?? new List<DetalleDevolucionDto>()).Select(d => new DevolucionDetalleItemViewModel
            {
                IdProducto = d.IdProducto,
                NombreProducto = d.NombreProducto,
                Cantidad = d.Cantidad,
                PrecioVenta = d.PrecioVenta
            }).ToList();
        }

        public List<ProductoVentaViewModel> ObtenerProductosVenta(int idVenta)
        {
            var r = Ejecutar(c => c.ObtenerProductosVenta(idVenta));
            ValidarExito(r.Exito, r.Mensaje);
            return (r.Productos ?? new List<ProductoVentaDto>()).Select(p => new ProductoVentaViewModel
            {
                IdProducto = p.IdProducto,
                NombreProducto = p.NombreProducto,
                CantidadVendida = p.CantidadVendida,
                CantidadDevuelta = p.CantidadDevuelta,
                CantidadPendiente = p.CantidadPendiente,
                PrecioVenta = p.PrecioVenta
            }).ToList();
        }

        /// <summary>
        /// Read operations report business errors (invalid or unknown id) as Exito=false;
        /// the service message is shown to the user as-is.
        /// </summary>
        private static void ValidarExito(bool exito, string mensaje)
        {
            if (!exito) throw new ErrorServicioException(mensaje, null);
        }

        public ResultadoDevolucion RegistrarDevolucion(int idVenta, string motivo, List<ItemDetalleViewModel> items)
        {
            var dtos = items.Select(i => new ItemDevolucionDto
            {
                IdProducto = i.IdProducto,
                Cantidad = i.Cantidad,
                ReingresaInventario = i.ReingresaInventario
            }).ToList();

            var r = Ejecutar(c => c.RegistrarDevolucionVenta(idVenta, motivo, dtos));
            return new ResultadoDevolucion
            {
                Exito = r.Exito,
                Mensaje = r.Mensaje,
                IdDevolucion = r.IdDevolucion,
                TipoDevolucion = r.TipoDevolucion,
                MontoTotalDevuelto = r.MontoTotalDevuelto
            };
        }

        /// <summary>
        /// Runs one call, always closes the channel and turns WCF errors into a friendly
        /// ErrorServicioException. Technical details only go to the trace log.
        /// </summary>
        private static T Ejecutar<T>(Func<ProductoServiceClient, T> llamada)
        {
            ProductoServiceClient cliente = null;
            try
            {
                cliente = ProductoServiceFactory.Crear();
                return llamada(cliente);
            }
            catch (MessageSecurityException ex)
            {
                throw Error("El servicio rechazó la autenticación del cliente. Revise la configuración de credenciales.", ex);
            }
            catch (SecurityNegotiationException ex)
            {
                throw Error("No se pudo establecer una conexión segura (HTTPS) con el servicio.", ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw Error("No se pudo conectar con el servicio. Verifique que esté en ejecución.", ex);
            }
            catch (TimeoutException ex)
            {
                throw Error("El servicio no respondió a tiempo. Intente nuevamente.", ex);
            }
            catch (FaultException ex)
            {
                throw Error("El servicio devolvió un error inesperado. Intente nuevamente.", ex);
            }
            catch (CommunicationException ex)
            {
                throw Error("Ocurrió un error de comunicación con el servicio. Intente nuevamente.", ex);
            }
            finally
            {
                ProductoServiceFactory.Cerrar(cliente);
            }
        }

        private static ErrorServicioException Error(string mensaje, Exception ex)
        {
            Trace.TraceError("DevolucionesServicio: " + ex);
            return new ErrorServicioException(mensaje, ex);
        }
    }
}
