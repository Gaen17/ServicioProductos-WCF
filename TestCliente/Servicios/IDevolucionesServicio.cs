using System.Collections.Generic;
using TestCliente.Models.Devoluciones;

namespace TestCliente.Servicios
{
    /// <summary>
    /// Returns operations used by DevolucionesController. Works with client view models,
    /// so the controller and views do not change when the proxy is regenerated.
    /// Throws ErrorServicioException on connection failure or when the service rejects the request.
    /// </summary>
    public interface IDevolucionesServicio
    {
        List<DevolucionResumenViewModel> ListarDevoluciones();
        List<DevolucionDetalleItemViewModel> VerDevolucionDetalle(int idDevolucion);
        List<ProductoVentaViewModel> ObtenerProductosVenta(int idVenta);
        ResultadoDevolucion RegistrarDevolucion(int idVenta, string motivo, List<ItemDetalleViewModel> items);
    }
}
