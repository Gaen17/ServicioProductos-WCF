using System;

namespace TestCliente.Servicios
{
    /// <summary>
    /// Connection or service failure. Message is user-friendly: no technical details or credentials.
    /// </summary>
    public class ErrorServicioException : Exception
    {
        public ErrorServicioException(string mensaje, Exception interna) : base(mensaje, interna) { }
    }

    /// <summary>The service does not publish this operation yet.</summary>
    public class OperacionNoDisponibleException : Exception
    {
        public OperacionNoDisponibleException(string operacion)
            : base("La operación " + operacion + " aún no está disponible en el servicio. Estará habilitada cuando se actualice el proxy.") { }
    }
}
