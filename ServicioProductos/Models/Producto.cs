using System.Runtime.Serialization;

namespace ServicioProductos
{
    /// <summary>
    /// Producto que se expone en el contrato del servicio.
    /// No es una entidad de Entity Framework.
    /// </summary>
    [DataContract]
    public class Producto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Descripcion { get; set; }

        [DataMember]
        public decimal Precio { get; set; }

        [DataMember]
        public int Stock { get; set; }

        [DataMember]
        public int IdCategoria { get; set; }

        [DataMember]
        public string CategoriaNombre { get; set; }

        [DataMember]
        public string UrlImagen { get; set; }
    }
}
