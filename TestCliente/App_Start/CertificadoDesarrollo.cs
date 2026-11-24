using System;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Web.Configuration;

namespace TestCliente
{
    /// <summary>
    /// DEVELOPMENT ONLY. Accepts the self-signed certificate of the local IIS service
    /// when it is not in Trusted Root. Active only when all of these hold:
    ///   - appSettings ServicioProductos.PermitirCertificadoAutofirmado = true
    ///   - compilation debug="true"
    ///   - the service URL host is localhost
    /// Even then, only certificate errors for requests to localhost are ignored.
    /// RISK: disabling certificate validation allows man-in-the-middle attacks and would
    /// expose the Basic credentials. Never enable it in production.
    /// </summary>
    public static class CertificadoDesarrollo
    {
        public static void Configurar()
        {
            bool permitir;
            bool.TryParse(ConfigurationManager.AppSettings["ServicioProductos.PermitirCertificadoAutofirmado"], out permitir);
            var compilacion = (CompilationSection)ConfigurationManager.GetSection("system.web/compilation");
            Uri url;
            var esLocal = Uri.TryCreate(ConfigurationManager.AppSettings["ServicioProductos.Url"], UriKind.Absolute, out url)
                          && url.IsLoopback;

            if (!permitir || compilacion == null || !compilacion.Debug || !esLocal) return;

            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errores) =>
            {
                if (errores == SslPolicyErrors.None) return true;
                var request = sender as HttpWebRequest;
                return request != null && request.RequestUri.IsLoopback;
            };
        }
    }
}
