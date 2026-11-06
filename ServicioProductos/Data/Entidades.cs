using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioProductos.Data
{
    /// <summary>
    /// Entidades del modelo de Entity Framework.
    /// El servicio no las devuelve: solo se usan para mapear la base
    /// y para que el contexto represente el esquema existente.
    /// </summary>
    [Table("Categorias")]
    public class CategoriaEntidad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        public virtual ICollection<ProductoEntidad> Productos { get; set; }
    }

    [Table("Productos")]
    public class ProductoEntidad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int IdCategoria { get; set; }

        [StringLength(500)]
        public string UrlImagen { get; set; }

        [ForeignKey("IdCategoria")]
        public virtual CategoriaEntidad Categoria { get; set; }
    }

    [Table("Ventas")]
    public class VentaEntidad
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public virtual ICollection<DetalleVentaEntidad> Detalles { get; set; }
    }

    [Table("DetalleVenta")]
    public class DetalleVentaEntidad
    {
        [Key]
        public int Id { get; set; }

        public int IdVenta { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal Precio { get; set; }

        [ForeignKey("IdVenta")]
        public virtual VentaEntidad Venta { get; set; }

        [ForeignKey("IdProducto")]
        public virtual ProductoEntidad Producto { get; set; }
    }

    [Table("Devolucion")]
    public class DevolucionEntidad
    {
        [Key]
        public int Id { get; set; }

        public int IdVenta { get; set; }

        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(255)]
        public string MotivoDevolucion { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoDevolucion { get; set; }

        public decimal MontoTotalDevuelto { get; set; }

        [ForeignKey("IdVenta")]
        public virtual VentaEntidad Venta { get; set; }
    }

    [Table("DetalleDevolucion")]
    public class DetalleDevolucionEntidad
    {
        [Key]
        public int Id { get; set; }

        public int IdDevolucion { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioVenta { get; set; }

        [ForeignKey("IdDevolucion")]
        public virtual DevolucionEntidad Devolucion { get; set; }

        [ForeignKey("IdProducto")]
        public virtual ProductoEntidad Producto { get; set; }
    }
}
