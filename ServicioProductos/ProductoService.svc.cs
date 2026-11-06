using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using ServicioProductos.Data;

namespace ServicioProductos
{
    /// <summary>
    /// Implementacion del servicio WCF. Las validaciones y los mensajes
    /// se resuelven aqui; el acceso a datos siempre pasa por la capa Data.
    /// </summary>
    public class ProductoService : IProductoService
    {
        private readonly ProductoData _productoData = new ProductoData();

        public RespuestaListaProductos ListarProductos()
        {
            try
            {
                var productos = _productoData.Listar().Select(Mapear).ToList();

                return new RespuestaListaProductos
                {
                    Exito = true,
                    Mensaje = productos.Count == 0
                        ? "No hay productos registrados."
                        : "Lista de productos obtenida correctamente.",
                    Productos = productos
                };
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error al listar productos: " + ex);
                return new RespuestaListaProductos
                {
                    Exito = false,
                    Mensaje = "No se pudo obtener la lista de productos. Intente nuevamente.",
                    Productos = new List<Producto>()
                };
            }
        }

        public RespuestaProducto ObtenerProducto(int id)
        {
            if (id <= 0)
            {
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "El identificador del producto debe ser mayor que cero."
                };
            }

            try
            {
                var producto = _productoData.Obtener(id);
                if (producto == null)
                {
                    return new RespuestaProducto
                    {
                        Exito = false,
                        Mensaje = "No se encontro un producto con el Id " + id + "."
                    };
                }

                return new RespuestaProducto
                {
                    Exito = true,
                    Mensaje = "Producto obtenido correctamente.",
                    Producto = Mapear(producto)
                };
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error al obtener el producto " + id + ": " + ex);
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "No se pudo obtener el producto. Intente nuevamente."
                };
            }
        }

        public RespuestaProducto InsertarProducto(Producto producto)
        {
            if (producto == null)
            {
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "Los datos del producto son obligatorios."
                };
            }

            Normalizar(producto);

            string error;
            if (!ValidarProducto(producto, out error))
            {
                return new RespuestaProducto { Exito = false, Mensaje = error };
            }

            try
            {
                var id = _productoData.Insertar(
                    producto.Nombre,
                    producto.Descripcion,
                    producto.Precio,
                    producto.Stock,
                    producto.IdCategoria,
                    producto.UrlImagen);

                if (id <= 0)
                {
                    return new RespuestaProducto
                    {
                        Exito = false,
                        Mensaje = "No se pudo registrar el producto."
                    };
                }

                return new RespuestaProducto
                {
                    Exito = true,
                    Mensaje = "Producto registrado correctamente.",
                    Producto = Mapear(_productoData.Obtener(id))
                };
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                Trace.TraceError("Error de clave foranea al insertar producto: " + ex);
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "La categoria indicada no existe."
                };
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error al insertar producto: " + ex);
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "No se pudo registrar el producto. Intente nuevamente."
                };
            }
        }

        public RespuestaProducto ActualizarProducto(Producto producto)
        {
            if (producto == null)
            {
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "Los datos del producto son obligatorios."
                };
            }

            if (producto.Id <= 0)
            {
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "El identificador del producto es obligatorio."
                };
            }

            try
            {
                if (_productoData.Obtener(producto.Id) == null)
                {
                    return new RespuestaProducto
                    {
                        Exito = false,
                        Mensaje = "No se encontro un producto con el Id " + producto.Id + "."
                    };
                }

                Normalizar(producto);

                string error;
                if (!ValidarProducto(producto, out error))
                {
                    return new RespuestaProducto { Exito = false, Mensaje = error };
                }

                var filas = _productoData.Actualizar(
                    producto.Id,
                    producto.Nombre,
                    producto.Descripcion,
                    producto.Precio,
                    producto.Stock,
                    producto.IdCategoria,
                    producto.UrlImagen);

                if (filas <= 0)
                {
                    return new RespuestaProducto
                    {
                        Exito = false,
                        Mensaje = "No se encontro un producto con el Id " + producto.Id + "."
                    };
                }

                return new RespuestaProducto
                {
                    Exito = true,
                    Mensaje = "Producto actualizado correctamente.",
                    Producto = Mapear(_productoData.Obtener(producto.Id))
                };
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                Trace.TraceError("Error de clave foranea al actualizar producto: " + ex);
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "La categoria indicada no existe."
                };
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error al actualizar el producto " + producto.Id + ": " + ex);
                return new RespuestaProducto
                {
                    Exito = false,
                    Mensaje = "No se pudo actualizar el producto. Intente nuevamente."
                };
            }
        }

        public RespuestaOperacion EliminarProducto(int id)
        {
            if (id <= 0)
            {
                return new RespuestaOperacion
                {
                    Exito = false,
                    Mensaje = "El identificador del producto debe ser mayor que cero."
                };
            }

            try
            {
                if (_productoData.Obtener(id) == null)
                {
                    return new RespuestaOperacion
                    {
                        Exito = false,
                        Mensaje = "No se encontro un producto con el Id " + id + "."
                    };
                }

                if (_productoData.TieneVentas(id))
                {
                    return new RespuestaOperacion
                    {
                        Exito = false,
                        Mensaje = "No se puede eliminar el producto porque tiene ventas asociadas."
                    };
                }

                var filas = _productoData.Eliminar(id);
                if (filas <= 0)
                {
                    return new RespuestaOperacion
                    {
                        Exito = false,
                        Mensaje = "No se encontro un producto con el Id " + id + "."
                    };
                }

                return new RespuestaOperacion
                {
                    Exito = true,
                    Mensaje = "Producto eliminado correctamente."
                };
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                Trace.TraceError("Error de clave foranea al eliminar el producto " + id + ": " + ex);
                return new RespuestaOperacion
                {
                    Exito = false,
                    Mensaje = "No se puede eliminar el producto porque tiene ventas asociadas."
                };
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error al eliminar el producto " + id + ": " + ex);
                return new RespuestaOperacion
                {
                    Exito = false,
                    Mensaje = "No se pudo eliminar el producto. Intente nuevamente."
                };
            }
        }

        private bool ValidarProducto(Producto producto, out string mensaje)
        {
            mensaje = null;

            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                mensaje = "El nombre del producto es obligatorio.";
                return false;
            }

            if (producto.Nombre.Length > 100)
            {
                mensaje = "El nombre del producto no debe superar los 100 caracteres.";
                return false;
            }

            if (producto.IdCategoria <= 0)
            {
                mensaje = "Debe seleccionar una categoria valida.";
                return false;
            }

            if (!_productoData.CategoriaExiste(producto.IdCategoria))
            {
                mensaje = "La categoria indicada no existe.";
                return false;
            }

            if (producto.Precio <= 0)
            {
                mensaje = "El precio debe ser mayor que cero.";
                return false;
            }

            if (producto.Stock < 0)
            {
                mensaje = "El stock debe ser mayor o igual que cero.";
                return false;
            }

            if (producto.Descripcion != null && producto.Descripcion.Length > 255)
            {
                mensaje = "La descripcion no debe superar los 255 caracteres.";
                return false;
            }

            if (producto.UrlImagen != null && producto.UrlImagen.Length > 500)
            {
                mensaje = "La URL de la imagen no debe superar los 500 caracteres.";
                return false;
            }

            return true;
        }

        private static void Normalizar(Producto producto)
        {
            if (producto.Nombre != null)
            {
                producto.Nombre = producto.Nombre.Trim();
            }

            producto.Descripcion = string.IsNullOrWhiteSpace(producto.Descripcion)
                ? null
                : producto.Descripcion.Trim();

            producto.UrlImagen = string.IsNullOrWhiteSpace(producto.UrlImagen)
                ? null
                : producto.UrlImagen.Trim();
        }

        private static Producto Mapear(ProductoSp producto)
        {
            if (producto == null)
            {
                return null;
            }

            return new Producto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                IdCategoria = producto.IdCategoria,
                CategoriaNombre = producto.CategoriaNombre,
                UrlImagen = producto.UrlImagen
            };
        }
    }
}
