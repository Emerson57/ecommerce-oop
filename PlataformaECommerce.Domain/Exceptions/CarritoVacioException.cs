using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class CarritoVacioException : CartException
    {
        /// Inicializa una nueva instancia de la excepción
        /// con el mensaje por defecto.
        public CarritoVacioException()
            : base("No es posible realizar la operación porque el carrito se encuentra vacío.")
        {
        }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje personalizado.
        public CarritoVacioException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje y una excepción interna.
        public CarritoVacioException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}