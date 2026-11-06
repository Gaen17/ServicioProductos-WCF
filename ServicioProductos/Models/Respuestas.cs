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

}
