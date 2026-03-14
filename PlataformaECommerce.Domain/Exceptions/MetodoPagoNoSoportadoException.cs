using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class MetodoPagoNoSoportadoException : PaymentException
    {
        /// Obtiene el nombre del método de pago que provocó la excepción.
        public string MetodoPago { get; }

        /// Inicializa una nueva instancia de la excepción indicando
        /// el método de pago no soportado.
        public MetodoPagoNoSoportadoException(string metodoPago)
            : base($"El método de pago '{NormalizarMetodoPago(metodoPago)}' no está soportado por el sistema.")
        {
            MetodoPago = NormalizarMetodoPago(metodoPago);
        }

        /// Inicializa una nueva instancia de la excepción con un mensaje personalizado
        /// y el método de pago que causó el error.
        public MetodoPagoNoSoportadoException(string message, string metodoPago)
            : base(message)
        {
            MetodoPago = NormalizarMetodoPago(metodoPago);
        }

        /// Inicializa una nueva instancia de la excepción con un mensaje personalizado,
        /// el método de pago y una excepción interna que provocó el error.
        public MetodoPagoNoSoportadoException(string message, string metodoPago, Exception innerException)
            : base(message, innerException)
        {
            MetodoPago = NormalizarMetodoPago(metodoPago);
        }

        #region Métodos privados auxiliares

        /// Valida y normaliza el nombre del método de pago recibido.
        private static string NormalizarMetodoPago(string metodoPago)
        {
            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new ArgumentNullException(nameof(metodoPago),
                    "El método de pago no puede ser nulo o vacío.");

            return metodoPago.Trim();
        }

        #endregion
    }
}