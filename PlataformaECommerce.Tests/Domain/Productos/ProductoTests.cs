using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Domain.Productos
{
    [TestFixture]
    public class ProductoTests
    {
        #region Método auxiliar

        /// Crea una instancia válida de ProductoDigital para reutilizar
        /// en las pruebas del comportamiento base de Producto.
        private static ProductoDigital CrearProductoValido()
        {
            return new ProductoDigital(
                id: 1,
                nombre: "Ebook C#",
                descripcion: "Guía completa de C# para principiantes.",
                precio: 50000m,
                stock: 10,
                formatoArchivo: "PDF",
                tamanoMB: 15.5m
            );
        }

        #endregion

        #region Pruebas de creación

        [Test]
        public void Constructor_DatosValidos_CreaProductoCorrectamente()
        {
            // Arrange & Act
            var producto = CrearProductoValido();

            // Assert
            Assert.That(producto.Id, Is.EqualTo(1));
            Assert.That(producto.Nombre, Is.EqualTo("Ebook C#"));
            Assert.That(producto.Descripcion, Is.EqualTo("Guía completa de C# para principiantes."));
            Assert.That(producto.Precio, Is.EqualTo(50000m));
            Assert.That(producto.Stock, Is.EqualTo(10));
            Assert.That(producto.Activo, Is.True);
            Assert.That(producto.EstaDisponible(), Is.True);
        }

        [Test]
        public void Constructor_IdInvalido_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoDigital(
                    id: 0,
                    nombre: "Ebook C#",
                    descripcion: "Guía completa de C# para principiantes.",
                    precio: 50000m,
                    stock: 10,
                    formatoArchivo: "PDF",
                    tamanoMB: 15.5m
                ));

            Assert.That(ex!.Message, Does.Contain("Id del producto"));
        }

        [Test]
        public void Constructor_PrecioInvalido_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoDigital(
                    id: 1,
                    nombre: "Ebook C#",
                    descripcion: "Guía completa de C# para principiantes.",
                    precio: 0m,
                    stock: 10,
                    formatoArchivo: "PDF",
                    tamanoMB: 15.5m
                ));

            Assert.That(ex!.Message, Does.Contain("precio"));
        }

        [Test]
        public void Constructor_StockNegativo_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoDigital(
                    id: 1,
                    nombre: "Ebook C#",
                    descripcion: "Guía completa de C# para principiantes.",
                    precio: 50000m,
                    stock: -1,
                    formatoArchivo: "PDF",
                    tamanoMB: 15.5m
                ));

            Assert.That(ex!.Message, Does.Contain("stock"));
        }

        #endregion

        #region Pruebas de actualización de precio

        [Test]
        public void ActualizarPrecio_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act
            producto.ActualizarPrecio(65000m);

            // Assert
            Assert.That(producto.Precio, Is.EqualTo(65000m));
        }

        [Test]
        public void ActualizarPrecio_ValorInvalido_LanzaProductException()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() => producto.ActualizarPrecio(-1000m));

            Assert.That(ex!.Message, Does.Contain("precio"));
        }

        #endregion

        #region Pruebas de stock

        [Test]
        public void ReponerStock_CantidadValida_IncrementaStock()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act
            producto.ReponerStock(5);

            // Assert
            Assert.That(producto.Stock, Is.EqualTo(15));
        }

        [Test]
        public void ReponerStock_CantidadInvalida_LanzaProductException()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() => producto.ReponerStock(0));

            Assert.That(ex!.Message, Does.Contain("reponer"));
        }

        [Test]
        public void ReducirStock_CantidadValida_DisminuyeStock()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act
            producto.ReducirStock(4);

            // Assert
            Assert.That(producto.Stock, Is.EqualTo(6));
        }

        [Test]
        public void ReducirStock_CantidadMayorAlStock_LanzaInventarioInsuficienteException()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act & Assert
            var ex = Assert.Throws<InventarioInsuficienteException>(() => producto.ReducirStock(50));

            Assert.That(ex!.Message, Does.Contain("Inventario insuficiente"));
        }

        [Test]
        public void ActualizarStock_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act
            producto.ActualizarStock(25);

            // Assert
            Assert.That(producto.Stock, Is.EqualTo(25));
        }

        [Test]
        public void ActualizarStock_ValorNegativo_LanzaProductException()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() => producto.ActualizarStock(-5));

            Assert.That(ex!.Message, Does.Contain("stock"));
        }

        #endregion

        #region Pruebas de estado y disponibilidad

        [Test]
        public void Desactivar_ProductoActivo_CambiaEstadoAInactivo()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act
            producto.Desactivar();

            // Assert
            Assert.That(producto.Activo, Is.False);
            Assert.That(producto.EstaDisponible(), Is.False);
        }

        [Test]
        public void Activar_ProductoInactivo_CambiaEstadoAActivo()
        {
            // Arrange
            var producto = CrearProductoValido();
            producto.Desactivar();

            // Act
            producto.Activar();

            // Assert
            Assert.That(producto.Activo, Is.True);
            Assert.That(producto.EstaDisponible(), Is.True);
        }

        [Test]
        public void EstaDisponible_ProductoSinStock_RetornaFalse()
        {
            // Arrange
            var producto = CrearProductoValido();
            producto.ActualizarStock(0);

            // Act & Assert
            Assert.That(producto.EstaDisponible(), Is.False);
        }

        #endregion

        #region Pruebas de información básica

        [Test]
        public void ActualizarInformacionBasica_DatosValidos_ActualizaNombreYDescripcion()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act
            producto.ActualizarInformacionBasica(
                "Nuevo Ebook C#",
                "Contenido actualizado y ampliado."
            );

            // Assert
            Assert.That(producto.Nombre, Is.EqualTo("Nuevo Ebook C#"));
            Assert.That(producto.Descripcion, Is.EqualTo("Contenido actualizado y ampliado."));
        }

        [Test]
        public void ActualizarInformacionBasica_NombreInvalido_LanzaProductException()
        {
            // Arrange
            var producto = CrearProductoValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                producto.ActualizarInformacionBasica("", "Descripción válida"));

            Assert.That(ex!.Message, Does.Contain("nombre"));
        }

        #endregion
    }
}