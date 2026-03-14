using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class PagoFallidoException : PaymentException
    {
        /// Monto asociado al intento de pago fallido.
        public decimal Monto { get; }

        /// Método de pago utilizado en el intento fallido.
        public string MetodoPago { get; }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje genérico por defecto.
        public PagoFallidoException()
            : base("El pago no pudo procesarse correctamente.")
        {
            Monto = 0m;
            MetodoPago = string.Empty;
        }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje personalizado.
        public PagoFallidoException(string message)
            : base(message)
        {
            Monto = 0m;
            MetodoPago = string.Empty;
        }

        /// Inicializa una nueva instancia de la excepción
        /// con detalles del intento de pago fallido.
        public PagoFallidoException(string message, decimal monto, string metodoPago)
            : base(message)
        {
            Monto = monto;
            MetodoPago = metodoPago?.Trim() ?? string.Empty;
        }

        /// Inicializa una nueva instancia de la excepción
        /// con un mensaje personalizado y una excepción interna.
        public PagoFallidoException(string message, Exception innerException)
            : base(message, innerException)
        {
            Monto = 0m;
            MetodoPago = string.Empty;
        }

        /// Inicializa una nueva instancia de la excepción
        /// con toda la información disponible del error.
        public PagoFallidoException(string message, decimal monto, string metodoPago, Exception innerException)
            : base(message, innerException)
        {
            Monto = monto;
            MetodoPago = metodoPago?.Trim() ?? string.Empty;
        }
    }
}