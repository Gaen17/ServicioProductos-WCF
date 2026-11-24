using System;
using System.Collections.Generic;

namespace TestCliente.Models.Devoluciones
{
    // Client-owned view models. Views never receive proxy classes directly.

    /// <summary>Row of the returns list (ListarDevoluciones).</summary>
    public class DevolucionResumenViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int IdVenta { get; set; }
        public string MotivoDevolucion { get; set; }
        public string TipoDevolucion { get; set; }
        public decimal MontoTotalDevuelto { get; set; }
    }

    /// <summary>Returned product of one return (VerDevolucionDetalle).</summary>
    public class DevolucionDetalleItemViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Subtotal { get { return Cantidad * PrecioVenta; } }
    }

    public class DevolucionDetalleViewModel
    {
        public int IdDevolucion { get; set; }
        public List<DevolucionDetalleItemViewModel> Items { get; set; } = new List<DevolucionDetalleItemViewModel>();
    }

    /// <summary>Product of a sale (ObtenerProductosVenta) plus the user's input for the form.</summary>
    public class ProductoVentaViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadVendida { get; set; }
        public int CantidadDevuelta { get; set; }
        public int CantidadPendiente { get; set; }
        public decimal PrecioVenta { get; set; }

        // Form input.
        public bool Seleccionado { get; set; }
        public string CantidadADevolver { get; set; }
        public bool ReingresaInventario { get; set; } = true;
    }

    /// <summary>Line already added to the return detail (sent to RegistrarDevolucionVenta).</summary>
    public class ItemDetalleViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool ReingresaInventario { get; set; }
        public decimal Subtotal { get { return Cantidad * PrecioVenta; } }
    }

    public class NuevaDevolucionViewModel
    {
        // Strings so non-numeric input can be reported with a clear message.
        public string IdVenta { get; set; }
        /// <summary>Sale whose products are loaded; detects a sale id changed without searching again.</summary>
        public int? IdVentaBuscada { get; set; }
        public string Motivo { get; set; }
        public List<ProductoVentaViewModel> Productos { get; set; } = new List<ProductoVentaViewModel>();
        public List<ItemDetalleViewModel> Detalle { get; set; } = new List<ItemDetalleViewModel>();

        public decimal TotalDetalle
        {
            get
            {
                decimal total = 0;
                foreach (var item in Detalle) total += item.Subtotal;
                return total;
            }
        }
    }

    /// <summary>Result of RegistrarDevolucionVenta, mapped from the proxy response.</summary>
    public class ResultadoDevolucion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public int IdDevolucion { get; set; }
        public string TipoDevolucion { get; set; }
        public decimal MontoTotalDevuelto { get; set; }
    }
}
