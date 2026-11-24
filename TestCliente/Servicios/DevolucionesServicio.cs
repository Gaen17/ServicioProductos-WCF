using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using TestCliente.Models.Devoluciones;
using TestCliente.Proxy;
using TestCliente.ServicioProductosRef;

namespace TestCliente.Servicios
{
    /// <summary>
    /// Implementation over the WCF proxy (HTTPS + Basic, via ProductoServiceFactory).
    /// </summary>
    public class DevolucionesServicio : IDevolucionesServicio
    {
        // TEMPORAL - reemplazar al regenerar el proxy (operation not in the current WSDL).
        public List<DevolucionResumenViewModel> ListarDevoluciones()
        {
            throw new OperacionNoDisponibleException("ListarDevoluciones");
        }

        // TEMPORAL - reemplazar al regenerar el proxy (operation not in the current WSDL).
        public List<DevolucionDetalleItemViewModel> VerDevolucionDetalle(int idDevolucion)
        {
            throw new OperacionNoDisponibleException("VerDevolucionDetalle");
        }

        // TEMPORAL - reemplazar al regenerar el proxy (operation not in the current WSDL).
        public List<ProductoVentaViewModel> ObtenerProductosVenta(int idVenta)
        {
            throw new OperacionNoDisponibleException("ObtenerProductosVenta");
        }

        public ResultadoDevolucion RegistrarDevolucion(int idVenta, string motivo, List<ItemDetalleViewModel> items)
        {
            var dtos = items.Select(i => new ItemDevolucionDto
            {
                IdProducto = i.IdProducto,
                Cantidad = i.Cantidad,
                ReingresaInventario = i.ReingresaInventario
            }).ToList();

            var r = Ejecutar(c => c.RegistrarDevolucionVenta(idVenta, motivo, dtos));
            return new ResultadoDevolucion
            {
                Exito = r.Exito,
                Mensaje = r.Mensaje,
                IdDevolucion = r.IdDevolucion,
                TipoDevolucion = r.TipoDevolucion,
                MontoTotalDevuelto = r.MontoTotalDevuelto
            };
        }

        /// <summary>
        /// Runs one call, always closes the channel and turns WCF errors into a friendly
        /// ErrorServicioException. Technical details only go to the trace log.
        /// </summary>
        private static T Ejecutar<T>(Func<ProductoServiceClient, T> llamada)
        {
            ProductoServiceClient cliente = null;
            try
            {
                cliente = ProductoServiceFactory.Crear();
                return llamada(cliente);
            }
            catch (MessageSecurityException ex)
            {
                throw Error("El servicio rechazó la autenticación del cliente. Revise la configuración de credenciales.", ex);
            }
            catch (SecurityNegotiationException ex)
            {
                throw Error("No se pudo establecer una conexión segura (HTTPS) con el servicio.", ex);
            }
            catch (EndpointNotFoundException ex)
            {
                throw Error("No se pudo conectar con el servicio. Verifique que esté en ejecución.", ex);
            }
            catch (TimeoutException ex)
            {
                throw Error("El servicio no respondió a tiempo. Intente nuevamente.", ex);
            }
            catch (FaultException ex)
            {
                throw Error("El servicio devolvió un error inesperado. Intente nuevamente.", ex);
            }
            catch (CommunicationException ex)
            {
                throw Error("Ocurrió un error de comunicación con el servicio. Intente nuevamente.", ex);
            }
            finally
            {
                ProductoServiceFactory.Cerrar(cliente);
            }
        }

        private static ErrorServicioException Error(string mensaje, Exception ex)
        {
            Trace.TraceError("DevolucionesServicio: " + ex);
            return new ErrorServicioException(mensaje, ex);
        }
    }
}
