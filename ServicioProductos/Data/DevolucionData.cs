using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServicioProductos.Data
{
    /// <summary>
    /// Acceso a datos de devoluciones. La cabecera, el detalle y el stock
    /// se graban dentro de una unica transaccion de Entity Framework.
    /// </summary>
    public class DevolucionData
    {
        public bool VentaExiste(int idVenta)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<int>(
                        "EXEC dbo.usp_Venta_Existe @IdVenta",
                        new SqlParameter("@IdVenta", SqlDbType.Int) { Value = idVenta })
                    .Single() == 1;
            }
        }

        public List<VentaPendienteSp> ObtenerPendientes(int idVenta)
        {
            using (var db = new DesavaPAContext())
            {
                return db.Database
                    .SqlQuery<VentaPendienteSp>(
                        "EXEC dbo.usp_Venta_ObtenerPendientes @IdVenta",
                        new SqlParameter("@IdVenta", SqlDbType.Int) { Value = idVenta })
                    .ToList();
            }
        }

        public int Registrar(int idVenta, string motivo, string tipo, decimal monto, List<DetalleDevolucionData> detalles)
        {
            using (var db = new DesavaPAContext())
            using (var transaccion = db.Database.BeginTransaction())
            {
                try
                {
                    var idDevolucion = db.Database
                        .SqlQuery<int>(
                            "EXEC dbo.usp_Devolucion_Insertar @IdVenta, @Motivo, @Tipo, @Monto",
                            new SqlParameter("@IdVenta", SqlDbType.Int) { Value = idVenta },
                            new SqlParameter("@Motivo", SqlDbType.VarChar, 255) { Value = motivo },
                            new SqlParameter("@Tipo", SqlDbType.VarChar, 20) { Value = tipo },
                            new SqlParameter("@Monto", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = monto })
                        .Single();

                    foreach (var detalle in detalles)
                    {
                        db.Database.ExecuteSqlCommand(
                            "EXEC dbo.usp_DetalleDevolucion_Insertar @IdDevolucion, @IdProducto, @Cantidad, @PrecioVenta",
                            new SqlParameter("@IdDevolucion", SqlDbType.Int) { Value = idDevolucion },
                            new SqlParameter("@IdProducto", SqlDbType.Int) { Value = detalle.IdProducto },
                            new SqlParameter("@Cantidad", SqlDbType.Int) { Value = detalle.Cantidad },
                            new SqlParameter("@PrecioVenta", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = detalle.PrecioVenta });

                        if (detalle.ReingresaInventario)
                        {
                            db.Database.ExecuteSqlCommand(
                                "EXEC dbo.usp_Producto_SumarStock @IdProducto, @Cantidad",
                                new SqlParameter("@IdProducto", SqlDbType.Int) { Value = detalle.IdProducto },
                                new SqlParameter("@Cantidad", SqlDbType.Int) { Value = detalle.Cantidad });
                        }
                    }

                    transaccion.Commit();
                    return idDevolucion;
                }
                catch
                {
                    try
                    {
                        transaccion.Rollback();
                    }
                    catch
                    {
                        // Si la transaccion ya no esta activa, se conserva la excepcion original.
                    }

                    throw;
                }
            }
        }
    }
}
