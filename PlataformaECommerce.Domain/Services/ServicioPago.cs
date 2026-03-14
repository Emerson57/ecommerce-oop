using System;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.Services
{
    /// Servicio de dominio encargado de procesar pagos de forma controlada
    /// dentro de la plataforma e-Commerce.
    public class ServicioPago
    {
        #region Métodos de negocio

        /// Procesa el pago de un carrito utilizando el método de pago indicado.
        public bool ProcesarPago(CarritoCompra carrito, MetodoPago metodoPago)
        {
            if (carrito is null)
                throw new PagoFallidoException("No es posible procesar el pago porque el carrito es nulo.");

            if (carrito.CantidadItems == 0)
                throw new CarritoVacioException("No es posible procesar el pago porque el carrito está vacío.");

            if (!carrito.Activo)
                throw new PagoFallidoException("No es posible procesar el pago porque el carrito está inactivo.");

            ValidarMetodoPagoSoportado(metodoPago);
            ValidarMonto(carrito.Total);

            // Simulación simple de regla de negocio:
            // Si el monto supera cierto umbral en transferencia, fallamos.
            if (metodoPago == MetodoPago.TransferenciaBancaria && carrito.Total > 10000000m)
            {
                throw new PagoFallidoException(
                    "El pago fue rechazado porque el monto excede el límite permitido para transferencia bancaria.",
                    carrito.Total,
                    metodoPago.ToString());
            }

            return true;
        }

        #endregion

        #region Métodos privados auxiliares

        /// Valida que el método de pago esté soportado por el sistema.
        private static void ValidarMetodoPagoSoportado(MetodoPago metodoPago)
        {
            if (!Enum.IsDefined(typeof(MetodoPago), metodoPago))
                throw new MetodoPagoNoSoportadoException(metodoPago.ToString());
        }

        /// Valida que el monto a pagar sea válido.
        private static void ValidarMonto(decimal monto)
        {
            if (monto <= 0)
            {
                throw new PagoFallidoException(
                    "No es posible procesar un pago con un monto menor o igual a cero.",
                    monto,
                    "N/A");
            }
        }

        #endregion
    }
}