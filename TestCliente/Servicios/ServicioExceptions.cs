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
}
