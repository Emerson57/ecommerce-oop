using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class PaymentException : DomainException
    {
        public PaymentException()
        {
        }

        /// Inicializa una nueva instancia de la excepción de pago
        /// con un mensaje descriptivo del error.
        public PaymentException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción de pago
        /// con un mensaje y una excepción interna.
        public PaymentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}