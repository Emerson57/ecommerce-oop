using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException()
        {
        }

        /// Inicializa una nueva instancia de la excepción de dominio
        /// con un mensaje de error específico.
        public DomainException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción de dominio
        /// con un mensaje de error y una excepción interna.
        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}