using System.ServiceModel;

namespace ServicioProductos
{
    [ServiceContract]
    public interface IProductoService
    {
        [OperationContract]
        RespuestaListaProductos ListarProductos();

        [OperationContract]
        RespuestaProducto ObtenerProducto(int id);

        [OperationContract]
        RespuestaProducto InsertarProducto(Producto producto);

        [OperationContract]
        RespuestaProducto ActualizarProducto(Producto producto);

        [OperationContract]
        RespuestaOperacion EliminarProducto(int id);
    }
}
