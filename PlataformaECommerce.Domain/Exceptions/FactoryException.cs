using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class FactoryException : DomainException
    {
        /// Inicializa una nueva instancia de la excepción de fábrica.
        public FactoryException()
        {
        }

        /// Inicializa una nueva instancia de la excepción de fábrica
        /// con un mensaje descriptivo del error.
        public FactoryException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción de fábrica
        /// con un mensaje y una excepción interna.
        public FactoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}