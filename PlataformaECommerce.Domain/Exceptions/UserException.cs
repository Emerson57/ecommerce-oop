using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class UserException : DomainException
    {
        /// Inicializa una nueva instancia de la excepción de usuario.
        public UserException()
        {
        }

        /// Inicializa una nueva instancia de la excepción de usuario
        /// con un mensaje descriptivo del error.
        public UserException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción de usuario
        /// con un mensaje y una excepción interna.
        public UserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}