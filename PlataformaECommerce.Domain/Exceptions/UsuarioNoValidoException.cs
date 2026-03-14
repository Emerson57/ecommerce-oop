using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class UsuarioNoValidoException : UserException
    {
        public int? UsuarioId { get; }

        /// Correo electrónico del usuario afectado, si está disponible.
        public string? Correo { get; }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje por defecto.
        public UsuarioNoValidoException()
            : base("El usuario no es válido para realizar la operación solicitada.")
        {
        }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje personalizado.
        public UsuarioNoValidoException(string message)
            : base(message)
        {
        }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje, identificador y correo del usuario.
        public UsuarioNoValidoException(string message, int? usuarioId, string? correo)
            : base(message)
        {
            UsuarioId = usuarioId;
            Correo = correo;
        }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje y una excepción interna.
        public UsuarioNoValidoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}