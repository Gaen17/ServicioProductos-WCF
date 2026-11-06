using System.Data.Entity;

namespace ServicioProductos.Data
{
    /// <summary>
    /// Contexto de Entity Framework sobre la base existente.
    /// No se usan migraciones ni inicializadores: el esquema lo crean
    /// los scripts SQL del proyecto.
    /// </summary>
    public class DesavaPAContext : DbContext
    {
        static DesavaPAContext()
        {
            Database.SetInitializer<DesavaPAContext>(null);
        }

        public DesavaPAContext() : base("name=DesavaPA")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<CategoriaEntidad> Categorias { get; set; }

        public DbSet<ProductoEntidad> Productos { get; set; }

        public DbSet<VentaEntidad> Ventas { get; set; }

        public DbSet<DetalleVentaEntidad> DetalleVenta { get; set; }

        public DbSet<DevolucionEntidad> Devoluciones { get; set; }

        public DbSet<DetalleDevolucionEntidad> DetalleDevolucion { get; set; }
    }
}
