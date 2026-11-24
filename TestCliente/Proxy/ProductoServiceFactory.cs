using System.Configuration;
using System.ServiceModel;
using TestCliente.ServicioProductosRef;

namespace TestCliente.Proxy
{
    /// <summary>
    /// Builds the WCF client using the Web.config binding (Transport + Basic)
    /// and the URL/credentials from appSettings.
    /// </summary>
    public static class ProductoServiceFactory
    {
        private const string EndpointName = "ProductoServiceEndpoint";

        public static ProductoServiceClient Crear()
        {
            var url = ConfigurationManager.AppSettings["ServicioProductos.Url"];
            var cliente = new ProductoServiceClient(EndpointName, new EndpointAddress(url));
            cliente.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["ServicioProductos.Usuario"];
            cliente.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["ServicioProductos.Clave"];
            return cliente;
        }

        /// <summary>
        /// Closes the channel; aborts it when it is faulted so it never throws on cleanup.
        /// </summary>
        public static void Cerrar(ICommunicationObject cliente)
        {
            if (cliente == null) return;
            try
            {
                if (cliente.State == CommunicationState.Faulted) cliente.Abort();
                else cliente.Close();
            }
            catch (CommunicationException) { cliente.Abort(); }
            catch (System.TimeoutException) { cliente.Abort(); }
        }
    }
}
