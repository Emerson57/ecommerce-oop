using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Application.Cart
{
    [TestFixture]
    public class CarritoCompraTests
    {
        #region Métodos auxiliares

        /// Crea un producto digital válido para reutilizar en las pruebas.
        private static ProductoDigital CrearProductoDigitalValido(
            int id = 1,
            string nombre = "Ebook C#",
            decimal precio = 50000m,
            int stock = 10)
        {
            return new ProductoDigital(
                id: id,
                nombre: nombre,
                descripcion: "Material digital de aprendizaje.",
                precio: precio,
                stock: stock,
                formatoArchivo: "PDF",
                tamanoMB: 10.5m
            );
        }

        /// Crea un producto físico válido para reutilizar en las pruebas.
        private static ProductoFisico CrearProductoFisicoValido(
            int id = 2,
            string nombre = "Mouse Gamer",
            decimal precio = 95000m,
            int stock = 5)
        {
            return new ProductoFisico(
                id: id,
                nombre: nombre,
                descripcion: "Mouse de alta precisión.",
                precio: precio,
                stock: stock,
                pesoKg: 0.2m,
                altoCm: 4m,
                anchoCm: 7m,
                largoCm: 12m
            );
        }

        #endregion

        #region Pruebas de creación

        [Test]
        public void Constructor_PorDefecto_CreaCarritoActivoYVacio()
        {
            // Arrange & Act
            var carrito = new CarritoCompra();

            // Assert
            Assert.That(carrito.Activo, Is.True);
            Assert.That(carrito.CantidadItems, Is.EqualTo(0));
            Assert.That(carrito.Total, Is.EqualTo(0m));
            Assert.That(carrito.Productos, Is.Empty);
        }

        [Test]
        public void Constructor_ActivoFalse_CreaCarritoInactivo()
        {
            // Arrange & Act
            var carrito = new CarritoCompra(activo: false);

            // Assert
            Assert.That(carrito.Activo, Is.False);
        }

        #endregion

        #region Pruebas de agregar productos

        [Test]
        public void AgregarProducto_ProductoValido_AgregaCorrectamente()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto = CrearProductoDigitalValido();

            // Act
            carrito.AgregarProducto(producto);

            // Assert
            Assert.That(carrito.CantidadItems, Is.EqualTo(1));
            Assert.That(carrito.Total, Is.EqualTo(producto.Precio));
            Assert.That(carrito.ContieneProducto(producto.Id), Is.True);
        }

        [Test]
        public void AgregarProducto_DosProductosValidos_CalculaTotalCorrectamente()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto1 = CrearProductoDigitalValido(id: 1, precio: 50000m);
            var producto2 = CrearProductoFisicoValido(id: 2, precio: 95000m);

            // Act
            carrito.AgregarProducto(producto1);
            carrito.AgregarProducto(producto2);

            // Assert
            Assert.That(carrito.CantidadItems, Is.EqualTo(2));
            Assert.That(carrito.Total, Is.EqualTo(145000m));
        }

        [Test]
        public void AgregarProducto_ProductoNulo_LanzaProductException()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() => carrito.AgregarProducto(null!));

            Assert.That(ex!.Message, Does.Contain("producto no puede ser nulo").IgnoreCase);
        }

        [Test]
        public void AgregarProducto_ProductoInactivo_LanzaProductoNoDisponibleException()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto = CrearProductoDigitalValido();
            producto.Desactivar();

            // Act & Assert
            Assert.Throws<ProductoNoDisponibleException>(() => carrito.AgregarProducto(producto));
        }

        [Test]
        public void AgregarProducto_ProductoSinStock_LanzaProductoNoDisponibleException()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto = CrearProductoDigitalValido(stock: 0);

            // Act & Assert
            Assert.Throws<ProductoNoDisponibleException>(() => carrito.AgregarProducto(producto));
        }

        [Test]
        public void AgregarProducto_CarritoInactivo_LanzaCartException()
        {
            // Arrange
            var carrito = new CarritoCompra(activo: false);
            var producto = CrearProductoDigitalValido();

            // Act & Assert
            var ex = Assert.Throws<CartException>(() => carrito.AgregarProducto(producto));

            Assert.That(ex!.Message, Does.Contain("carrito está inactivo").IgnoreCase);
        }

        [Test]
        public void AgregarProducto_SuperaMaximoItemsPermitidos_LanzaCartException()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act
            for (int i = 1; i <= 100; i++)
            {
                carrito.AgregarProducto(CrearProductoDigitalValido(id: i, precio: 1000m));
            }

            // Assert
            var ex = Assert.Throws<CartException>(() =>
                carrito.AgregarProducto(CrearProductoDigitalValido(id: 101, precio: 1000m)));

            Assert.That(ex!.Message, Does.Contain("más de 100 ítems").IgnoreCase);
        }

        #endregion

        #region Pruebas de remoción de productos

        [Test]
        public void RemoverProducto_ProductoExistente_RemueveCorrectamente()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto = CrearProductoDigitalValido();

            carrito.AgregarProducto(producto);

            // Act
            var resultado = carrito.RemoverProducto(producto.Id);

            // Assert
            Assert.That(resultado, Is.True);
            Assert.That(carrito.CantidadItems, Is.EqualTo(0));
            Assert.That(carrito.Total, Is.EqualTo(0m));
        }

        [Test]
        public void RemoverProducto_ProductoNoExistente_RetornaFalse()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act
            var resultado = carrito.RemoverProducto(999);

            // Assert
            Assert.That(resultado, Is.False);
        }

        [Test]
        public void RemoverProducto_IdInvalido_LanzaCartException()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act & Assert
            var ex = Assert.Throws<CartException>(() => carrito.RemoverProducto(0));

            Assert.That(ex!.Message, Does.Contain("Id del producto").IgnoreCase);
        }

        [Test]
        public void RemoverProducto_CarritoInactivo_LanzaCartException()
        {
            // Arrange
            var carrito = new CarritoCompra(activo: false);

            // Act & Assert
            var ex = Assert.Throws<CartException>(() => carrito.RemoverProducto(1));

            Assert.That(ex!.Message, Does.Contain("carrito está inactivo").IgnoreCase);
        }

        #endregion

        #region Pruebas de vaciado

        [Test]
        public void VaciarCarrito_ConProductos_EliminaTodosLosItemsYReiniciaTotal()
        {
            // Arrange
            var carrito = new CarritoCompra();
            carrito.AgregarProducto(CrearProductoDigitalValido(id: 1, precio: 50000m));
            carrito.AgregarProducto(CrearProductoFisicoValido(id: 2, precio: 95000m));

            // Act
            carrito.VaciarCarrito();

            // Assert
            Assert.That(carrito.CantidadItems, Is.EqualTo(0));
            Assert.That(carrito.Total, Is.EqualTo(0m));
            Assert.That(carrito.Productos, Is.Empty);
        }

        [Test]
        public void VaciarCarrito_CarritoVacio_LanzaCarritoVacioException()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act & Assert
            Assert.Throws<CarritoVacioException>(() => carrito.VaciarCarrito());
        }

        [Test]
        public void VaciarCarrito_CarritoInactivo_LanzaCartException()
        {
            // Arrange
            var carrito = new CarritoCompra(activo: false);

            // Act & Assert
            var ex = Assert.Throws<CartException>(() => carrito.VaciarCarrito());

            Assert.That(ex!.Message, Does.Contain("carrito está inactivo").IgnoreCase);
        }

        #endregion

        #region Pruebas de consulta y cálculo

        [Test]
        public void ContieneProducto_ProductoExistente_RetornaTrue()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto = CrearProductoDigitalValido();

            carrito.AgregarProducto(producto);

            // Act & Assert
            Assert.That(carrito.ContieneProducto(producto.Id), Is.True);
        }

        [Test]
        public void ContieneProducto_ProductoNoExistente_RetornaFalse()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act & Assert
            Assert.That(carrito.ContieneProducto(123), Is.False);
        }

        [Test]
        public void ObtenerCantidadDeProducto_ProductoRepetido_RetornaCantidadCorrecta()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto = CrearProductoDigitalValido();

            carrito.AgregarProducto(producto);
            carrito.AgregarProducto(producto);
            carrito.AgregarProducto(producto);

            // Act
            var cantidad = carrito.ObtenerCantidadDeProducto(producto.Id);

            // Assert
            Assert.That(cantidad, Is.EqualTo(3));
        }

        [Test]
        public void ObtenerCantidadDeProducto_IdInvalido_LanzaCartException()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act & Assert
            var ex = Assert.Throws<CartException>(() => carrito.ObtenerCantidadDeProducto(0));

            Assert.That(ex!.Message, Does.Contain("Id del producto").IgnoreCase);
        }

        [Test]
        public void ObtenerProductoPorId_ProductoExistente_RetornaProducto()
        {
            // Arrange
            var carrito = new CarritoCompra();
            var producto = CrearProductoDigitalValido();

            carrito.AgregarProducto(producto);

            // Act
            var resultado = carrito.ObtenerProductoPorId(producto.Id);

            // Assert
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.Id, Is.EqualTo(producto.Id));
        }

        [Test]
        public void ObtenerProductoPorId_IdInvalido_RetornaNull()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act
            var resultado = carrito.ObtenerProductoPorId(0);

            // Assert
            Assert.That(resultado, Is.Null);
        }

        [Test]
        public void CalcularTotal_ConProductos_RetornaTotalCorrecto()
        {
            // Arrange
            var carrito = new CarritoCompra();
            carrito.AgregarProducto(CrearProductoDigitalValido(id: 1, precio: 50000m));
            carrito.AgregarProducto(CrearProductoFisicoValido(id: 2, precio: 95000m));

            // Act
            var total = carrito.CalcularTotal();

            // Assert
            Assert.That(total, Is.EqualTo(145000m));
        }

        #endregion

        #region Pruebas de estado del carrito

        [Test]
        public void Desactivar_CarritoActivo_CambiaEstadoAInactivo()
        {
            // Arrange
            var carrito = new CarritoCompra();

            // Act
            carrito.Desactivar();

            // Assert
            Assert.That(carrito.Activo, Is.False);
        }

        [Test]
        public void Activar_CarritoInactivo_CambiaEstadoAActivo()
        {
            // Arrange
            var carrito = new CarritoCompra(activo: false);

            // Act
            carrito.Activar();

            // Assert
            Assert.That(carrito.Activo, Is.True);
        }

        #endregion
    }
}