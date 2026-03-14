using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class ConfiguracionInvalidaException : ConfigurationException
    {
        /// Nombre del parámetro de configuración afectado.
        public string Parametro { get; }

        /// Valor recibido para el parámetro, si está disponible.
        public object? ValorRecibido { get; }

        /// Inicializa una nueva instancia de la excepción con un
        /// mensaje por defecto.
        public ConfiguracionInvalidaException()
            : base("La configuración del sistema contiene un valor inválido.")
        {
            Parametro = string.Empty;
        }

        /// Inicializa una nueva instancia de la excepción con un
        /// mensaje descriptivo del error.
        public ConfiguracionInvalidaException(string message)
            : base(message)
        {
            Parametro = string.Empty;
        }

        /// Inicializa una nueva instancia de la excepción indicando
        /// el parámetro inválido y el valor recibido.
        public ConfiguracionInvalidaException(string parametro, object? valorRecibido)
            : base($"El parámetro de configuración '{parametro}' tiene un valor inválido: '{valorRecibido}'.")
        {
            Parametro = parametro ?? string.Empty;
            ValorRecibido = valorRecibido;
        }

        /// Inicializa una nueva instancia de la excepción con un mensaje,
        /// parámetro y valor recibido.
        public ConfiguracionInvalidaException(string message, string parametro, object? valorRecibido)
            : base(message)
        {
            Parametro = parametro ?? string.Empty;
            ValorRecibido = valorRecibido;
        }

        /// Inicializa una nueva instancia de la excepción con mensaje,
        /// excepción interna, parámetro y valor recibido.
        public ConfiguracionInvalidaException(string message, string parametro, object? valorRecibido, Exception innerException)
            : base(message, innerException)
        {
            Parametro = parametro ?? string.Empty;
            ValorRecibido = valorRecibido;
        }
    }
}