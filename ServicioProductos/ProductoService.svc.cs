using System.Collections.Generic;

namespace ServicioProductos
{
    public class ProductoService : IProductoService
    {
        private readonly DataProducto _datos = new DataProducto();

        public List<Producto> ListarProductos() => _datos.ObtenerTodos();
        public Producto ObtenerProducto(int id) => _datos.ObtenerPorId(id);
    }
}