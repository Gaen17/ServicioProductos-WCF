using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServicioProductos
{
    [ServiceContract]
    public interface IProductoService
    {
        [OperationContract]
        List<Producto> ListarProductos();

        [OperationContract]
        Producto ObtenerProducto(int id);
    }

    [DataContract]
    public class Producto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public decimal Precio { get; set; }
    }
}