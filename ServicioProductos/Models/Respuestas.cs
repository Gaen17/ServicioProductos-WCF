using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServicioProductos
{
    /// <summary>
    /// Respuesta de ListarProductos.
    /// </summary>
    [DataContract]
    public class RespuestaListaProductos
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public List<Producto> Productos { get; set; }
    }

    /// <summary>
    /// Respuesta de ObtenerProducto, InsertarProducto y ActualizarProducto.
    /// </summary>
    [DataContract]
    public class RespuestaProducto
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public Producto Producto { get; set; }
    }

    /// <summary>
    /// Respuesta de EliminarProducto.
    /// </summary>
    [DataContract]
    public class RespuestaOperacion
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }
    }

    /// <summary>
    /// Item de la lista que recibe RegistrarDevolucionVenta.
    /// </summary>
    [DataContract]
    public class ItemDevolucionDto
    {
        [DataMember]
        public int IdProducto { get; set; }

        [DataMember]
        public int Cantidad { get; set; }

        [DataMember]
        public bool ReingresaInventario { get; set; }
    }

    /// <summary>
    /// Respuesta de RegistrarDevolucionVenta.
    /// </summary>
    [DataContract]
    public class RespuestaDevolucion
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public int IdDevolucion { get; set; }

        [DataMember]
        public string TipoDevolucion { get; set; }

        [DataMember]
        public decimal MontoTotalDevuelto { get; set; }
    }

    /// <summary>
    /// Fila de ListarDevoluciones.
    /// </summary>
    [DataContract]
    public class DevolucionListadoDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public System.DateTime Fecha { get; set; }

        [DataMember]
        public int IdVenta { get; set; }

        [DataMember]
        public string MotivoDevolucion { get; set; }

        [DataMember]
        public string TipoDevolucion { get; set; }

        [DataMember]
        public decimal MontoTotalDevuelto { get; set; }
    }

    /// <summary>
    /// Respuesta de ListarDevoluciones.
    /// </summary>
    [DataContract]
    public class RespuestaListaDevoluciones
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public List<DevolucionListadoDto> Devoluciones { get; set; }
    }

    /// <summary>
    /// Fila de VerDevolucionDetalle.
    /// </summary>
    [DataContract]
    public class DetalleDevolucionDto
    {
        [DataMember]
        public int IdProducto { get; set; }

        [DataMember]
        public string NombreProducto { get; set; }

        [DataMember]
        public int Cantidad { get; set; }

        [DataMember]
        public decimal PrecioVenta { get; set; }
    }

    /// <summary>
    /// Respuesta de VerDevolucionDetalle.
    /// </summary>
    [DataContract]
    public class RespuestaDetalleDevolucion
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public List<DetalleDevolucionDto> Detalle { get; set; }
    }

    /// <summary>
    /// Fila de ObtenerProductosVenta.
    /// </summary>
    [DataContract]
    public class ProductoVentaDto
    {
        [DataMember]
        public int IdProducto { get; set; }

        [DataMember]
        public string NombreProducto { get; set; }

        [DataMember]
        public int CantidadVendida { get; set; }

        [DataMember]
        public int CantidadDevuelta { get; set; }

        [DataMember]
        public int CantidadPendiente { get; set; }

        [DataMember]
        public decimal PrecioVenta { get; set; }
    }

    /// <summary>
    /// Respuesta de ObtenerProductosVenta.
    /// </summary>
    [DataContract]
    public class RespuestaProductosVenta
    {
        [DataMember]
        public bool Exito { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public List<ProductoVentaDto> Productos { get; set; }
    }
}
