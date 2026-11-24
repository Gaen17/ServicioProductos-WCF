using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServicioProductos.Data
{
    /// <summary>
    /// Acceso a datos de productos. Todas las operaciones usan
    /// procedimientos almacenados con parametros.
    /// </summary>
    public class ProductoData
    {
        public List<ProductoSp> Listar()
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<ProductoSp>("EXEC dbo.usp_Producto_Listar")
                    .ToList();
            }
        }

        public ProductoSp Obtener(int id)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<ProductoSp>(
                        "EXEC dbo.usp_Producto_Obtener @Id",
                        new SqlParameter("@Id", SqlDbType.Int) { Value = id })
                    .FirstOrDefault();
            }
        }

        public int Insertar(string nombre, string descripcion, decimal precio, int stock, int idCategoria, string urlImagen)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<int>(
                        "EXEC dbo.usp_Producto_Insertar @Nombre, @Descripcion, @Precio, @Stock, @IdCategoria, @UrlImagen",
                        new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = nombre },
                        new SqlParameter("@Descripcion", SqlDbType.VarChar, 255) { Value = (object)descripcion ?? DBNull.Value },
                        new SqlParameter("@Precio", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = precio },
                        new SqlParameter("@Stock", SqlDbType.Int) { Value = stock },
                        new SqlParameter("@IdCategoria", SqlDbType.Int) { Value = idCategoria },
                        new SqlParameter("@UrlImagen", SqlDbType.VarChar, 500) { Value = (object)urlImagen ?? DBNull.Value })
                    .Single();
            }
        }

        public int Actualizar(int id, string nombre, string descripcion, decimal precio, int stock, int idCategoria, string urlImagen)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<int>(
                        "EXEC dbo.usp_Producto_Actualizar @Id, @Nombre, @Descripcion, @Precio, @Stock, @IdCategoria, @UrlImagen",
                        new SqlParameter("@Id", SqlDbType.Int) { Value = id },
                        new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = nombre },
                        new SqlParameter("@Descripcion", SqlDbType.VarChar, 255) { Value = (object)descripcion ?? DBNull.Value },
                        new SqlParameter("@Precio", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = precio },
                        new SqlParameter("@Stock", SqlDbType.Int) { Value = stock },
                        new SqlParameter("@IdCategoria", SqlDbType.Int) { Value = idCategoria },
                        new SqlParameter("@UrlImagen", SqlDbType.VarChar, 500) { Value = (object)urlImagen ?? DBNull.Value })
                    .Single();
            }
        }

        public int Eliminar(int id)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<int>(
                        "EXEC dbo.usp_Producto_Eliminar @Id",
                        new SqlParameter("@Id", SqlDbType.Int) { Value = id })
                    .Single();
            }
        }

        public bool CategoriaExiste(int idCategoria)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<int>(
                        "EXEC dbo.usp_Categoria_Existe @IdCategoria",
                        new SqlParameter("@IdCategoria", SqlDbType.Int) { Value = idCategoria })
                    .Single() == 1;
            }
        }

        public bool TieneVentas(int idProducto)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<int>(
                        "EXEC dbo.usp_Producto_TieneVentas @IdProducto",
                        new SqlParameter("@IdProducto", SqlDbType.Int) { Value = idProducto })
                    .Single() == 1;
            }
        }
    }
}
