using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class EntidadNoSoportadaException : FactoryException
    {
        public string TipoEntidad { get; }

        /// Categoría funcional de la entidad, si aplica.
        public string? Categoria { get; }

        /// Inicializa una nueva instancia de la excepción indicando
        /// el tipo de entidad no soportado.
        public EntidadNoSoportadaException(string tipoEntidad)
            : base($"La entidad solicitada '{tipoEntidad}' no está soportada por la fábrica.")
        {
            TipoEntidad = tipoEntidad ?? string.Empty;
        }

        /// Inicializa una nueva instancia de la excepción indicando
        /// el tipo y la categoría funcional de la entidad no soportada.
        public EntidadNoSoportadaException(string tipoEntidad, string? categoria)
            : base($"La entidad solicitada '{tipoEntidad}' no está soportada por la fábrica en la categoría '{categoria}'.")
        {
            TipoEntidad = tipoEntidad ?? string.Empty;
            Categoria = categoria;
        }

        /// Inicializa una nueva instancia de la excepción con un mensaje personalizado.
        public EntidadNoSoportadaException(string message, string tipoEntidad, string? categoria = null)
            : base(message)
        {
            TipoEntidad = tipoEntidad ?? string.Empty;
            Categoria = categoria;
        }

        /// Inicializa una nueva instancia de la excepción con mensaje,
        /// tipo de entidad, categoría y excepción interna.
        public EntidadNoSoportadaException(string message, string tipoEntidad, string? categoria, Exception innerException)
            : base(message, innerException)
        {
            TipoEntidad = tipoEntidad ?? string.Empty;
            Categoria = categoria;
        }
    }
}