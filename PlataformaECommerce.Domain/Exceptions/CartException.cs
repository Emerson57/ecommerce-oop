using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class CartException : DomainException
    {
        /// Inicializa una nueva instancia de la excepción del carrito.
        public CartException()
        {
        }

        /// Inicializa una nueva instancia de la excepción del carrito
        /// con un mensaje descriptivo del error.
        public CartException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción del carrito
        /// con un mensaje y una excepción interna.
        public CartException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}