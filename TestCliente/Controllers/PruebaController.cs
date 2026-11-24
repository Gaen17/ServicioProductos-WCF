using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Web.Mvc;
using TestCliente.Proxy;
using TestCliente.ServicioProductosRef;

namespace TestCliente.Controllers
{
    /// <summary>
    /// Connectivity test: calls ListarProductos over HTTPS with Basic credentials.
    /// </summary>
    public class PruebaController : Controller
    {
        public ActionResult Index()
        {
            ProductoServiceClient cliente = null;
            try
            {
                cliente = ProductoServiceFactory.Crear();
                var respuesta = cliente.ListarProductos();
                if (!respuesta.Exito) ViewBag.Error = "The service responded with an error: " + respuesta.Mensaje;
                return View(respuesta);
            }
            catch (MessageSecurityException ex)
            {
                // A 401 from IIS arrives here: wrong or missing Basic credentials.
                ViewBag.Error = "Authentication rejected (check user/password in Web.config): " + ex.Message;
            }
            catch (SecurityNegotiationException ex)
            {
                ViewBag.Error = "HTTPS channel could not be established (untrusted certificate?): " + ex.Message;
            }
            catch (EndpointNotFoundException ex)
            {
                ViewBag.Error = "Service not reachable (is the IIS site running?): " + ex.Message;
            }
            catch (TimeoutException ex)
            {
                ViewBag.Error = "The service did not respond in time: " + ex.Message;
            }
            catch (CommunicationException ex)
            {
                ViewBag.Error = "Communication error with the service: " + ex.Message;
            }
            finally
            {
                ProductoServiceFactory.Cerrar(cliente);
            }
            return View((RespuestaListaProductos)null);
        }
    }
}
