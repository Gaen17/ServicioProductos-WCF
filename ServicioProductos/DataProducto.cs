using System.Collections.Generic;
using System.Linq;

namespace ServicioProductos
{
    public class DataProducto
    {
        private readonly List<Producto> _productos = new List<Producto>

        {
            new Producto { Id = 1, Nombre = "Laptop", Precio = 3500m },
            new Producto { Id = 2, Nombre = "Mouse", Precio = 120m },
            new Producto { Id = 3, Nombre = "Teclado", Precio = 250m },
            new Producto { Id = 4, Nombre = "Monitor", Precio = 1800m },
            new Producto { Id = 5, Nombre = "Auriculares", Precio = 450m }
        };

        public List<Producto> ObtenerTodos() => _productos;
        public Producto ObtenerPorId(int id) => _productos.FirstOrDefault(p => p.Id == id);
    }
}