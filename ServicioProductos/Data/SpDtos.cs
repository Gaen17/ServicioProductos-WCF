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
}
