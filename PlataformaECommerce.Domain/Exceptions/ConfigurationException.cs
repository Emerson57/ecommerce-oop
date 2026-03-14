using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class ConfigurationException : DomainException
    {
        /// Inicializa una nueva instancia de la excepción de configuración.
        public ConfigurationException()
        {
        }

        /// Inicializa una nueva instancia de la excepción de configuración
        /// con un mensaje descriptivo del error.
        public ConfigurationException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción de configuración
        /// con un mensaje y una excepción interna.
        public ConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}