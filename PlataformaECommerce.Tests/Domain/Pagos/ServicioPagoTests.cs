using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.Services;

namespace PlataformaECommerce.Tests.Domain.Pagos
{
    [TestFixture]
    public class ServicioPagoTests
    {
        #region Métodos auxiliares

        /// Crea un carrito válido con al menos un producto.
        private static CarritoCompra CrearCarritoValido(decimal precioProducto = 50000m)
        {
            var carrito = new CarritoCompra();

            var producto = new ProductoDigital(
                id: 1,
                nombre: "Curso C#",
                descripcion: "Curso digital completo.",
                precio: precioProducto,
                stock: 10,
                formatoArchivo: "PDF",
                tamanoMB: 15m
            );

            carrito.AgregarProducto(producto);
            return carrito;
        }

        #endregion

        #region Pruebas de procesamiento exitoso

        [Test]
        public void ProcesarPago_CarritoValidoYMetodoSoportado_RetornaTrue()
        {
            // Arrange
            var servicioPago = new ServicioPago();
            var carrito = CrearCarritoValido();

            // Act
            var resultado = servicioPago.ProcesarPago(carrito, MetodoPago.TarjetaCredito);

            // Assert
            Assert.That(resultado, Is.True);
        }

        [Test]
        public void ProcesarPago_CarritoValidoConPSE_RetornaTrue()
        {
            // Arrange
            var servicioPago = new ServicioPago();
            var carrito = CrearCarritoValido();

            // Act
            var resultado = servicioPago.ProcesarPago(carrito, MetodoPago.PSE);

            // Assert
            Assert.That(resultado, Is.True);
        }

        #endregion

        #region Pruebas de errores del carrito

        [Test]
        public void ProcesarPago_CarritoNulo_LanzaPagoFallidoException()
        {
            // Arrange
            var servicioPago = new ServicioPago();

            // Act & Assert
            var ex = Assert.Throws<PagoFallidoException>(() =>
                servicioPago.ProcesarPago(null!, MetodoPago.TarjetaCredito));

            Assert.That(ex!.Message, Does.Contain("carrito es nulo").IgnoreCase);
        }

        [Test]
        public void ProcesarPago_CarritoVacio_LanzaCarritoVacioException()
        {
            // Arrange
            var servicioPago = new ServicioPago();
            var carrito = new CarritoCompra();

            // Act & Assert
            Assert.Throws<CarritoVacioException>(() =>
                servicioPago.ProcesarPago(carrito, MetodoPago.TarjetaCredito));
        }

        [Test]
        public void ProcesarPago_CarritoInactivo_LanzaPagoFallidoException()
        {
            // Arrange
            var servicioPago = new ServicioPago();
            var carrito = CrearCarritoValido();
            carrito.Desactivar();

            // Act & Assert
            var ex = Assert.Throws<PagoFallidoException>(() =>
                servicioPago.ProcesarPago(carrito, MetodoPago.TarjetaCredito));

            Assert.That(ex!.Message, Does.Contain("carrito está inactivo").IgnoreCase);
        }

        #endregion

        #region Pruebas de método de pago

        [Test]
        public void ProcesarPago_MetodoPagoNoDefinido_LanzaMetodoPagoNoSoportadoException()
        {
            // Arrange
            var servicioPago = new ServicioPago();
            var carrito = CrearCarritoValido();
            var metodoInvalido = (MetodoPago)999;

            // Act & Assert
            Assert.Throws<MetodoPagoNoSoportadoException>(() =>
                servicioPago.ProcesarPago(carrito, metodoInvalido));
        }

        #endregion

        #region Pruebas de fallos de negocio

        [Test]
        public void ProcesarPago_TransferenciaConMontoExcesivo_LanzaPagoFallidoException()
        {
            // Arrange
            var servicioPago = new ServicioPago();
            var carrito = CrearCarritoValido(precioProducto: 15000000m);

            // Act & Assert
            var ex = Assert.Throws<PagoFallidoException>(() =>
                servicioPago.ProcesarPago(carrito, MetodoPago.TransferenciaBancaria));

            Assert.That(ex!.Message, Does.Contain("excede el límite").IgnoreCase);
        }

        #endregion
    }
}