namespace ServicioProductos.Data
{
    /// <summary>
    /// Resultado de usp_Producto_Listar y usp_Producto_Obtener.
    /// </summary>
    public class ProductoSp
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int IdCategoria { get; set; }

        public string CategoriaNombre { get; set; }

        public string UrlImagen { get; set; }
    }

    /// <summary>
    /// Resultado de usp_Venta_ObtenerPendientes.
    /// </summary>
    public class VentaPendienteSp
    {
        public int IdProducto { get; set; }

        public int CantidadVendida { get; set; }

        public int CantidadDevuelta { get; set; }

        public int CantidadPendiente { get; set; }

        public decimal PrecioVenta { get; set; }
    }

    /// <summary>
    /// Detalle ya validado que se inserta dentro de la transaccion.
    /// </summary>
    public class DetalleDevolucionData
    {
        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioVenta { get; set; }

        public bool ReingresaInventario { get; set; }
    }

    /// <summary>
    /// Resultado de usp_Devolucion_Listar.
    /// </summary>
    public class DevolucionSp
    {
        public int Id { get; set; }

        public System.DateTime Fecha { get; set; }

        public int IdVenta { get; set; }

        public string MotivoDevolucion { get; set; }

        public string TipoDevolucion { get; set; }

        public decimal MontoTotalDevuelto { get; set; }
    }

    /// <summary>
    /// Resultado de usp_DetalleDevolucion_ObtenerPorDevolucion.
    /// </summary>
    public class DetalleDevolucionSp
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioVenta { get; set; }
    }

    /// <summary>
    /// Resultado de usp_Venta_ObtenerProductos.
    /// </summary>
    public class VentaProductoSp
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; }

        public int CantidadVendida { get; set; }

        public int CantidadDevuelta { get; set; }

        public int CantidadPendiente { get; set; }

        public decimal PrecioVenta { get; set; }
    }
}
