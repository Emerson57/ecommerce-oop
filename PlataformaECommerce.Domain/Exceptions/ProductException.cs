using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class ProductException : DomainException
    {
        /// Inicializa una nueva instancia de la excepción de producto.
        public ProductException()
        {
        }

        /// Inicializa una nueva instancia de la excepción de producto
        /// con un mensaje descriptivo del error.
        public ProductException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción de producto
        /// con un mensaje y una excepción interna.
        public ProductException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}