using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Domain.Productos
{
    [TestFixture]
    public class ProductoFisicoTests
    {
        #region Método auxiliar

        /// Crea una instancia válida de ProductoFisico para reutilizar en las pruebas.
        private static ProductoFisico CrearProductoFisicoValido()
        {
            return new ProductoFisico(
                id: 10,
                nombre: "Teclado Mecánico",
                descripcion: "Teclado mecánico con iluminación RGB.",
                precio: 350000m,
                stock: 20,
                pesoKg: 1.2m,
                altoCm: 4.5m,
                anchoCm: 18.0m,
                largoCm: 45.0m
            );
        }

        #endregion

        #region Pruebas de creación

        [Test]
        public void Constructor_DatosValidos_CreaProductoFisicoCorrectamente()
        {
            // Arrange & Act
            var producto = CrearProductoFisicoValido();

            // Assert
            Assert.That(producto.Id, Is.EqualTo(10));
            Assert.That(producto.Nombre, Is.EqualTo("Teclado Mecánico"));
            Assert.That(producto.PesoKg, Is.EqualTo(1.2m));
            Assert.That(producto.AltoCm, Is.EqualTo(4.5m));
            Assert.That(producto.AnchoCm, Is.EqualTo(18.0m));
            Assert.That(producto.LargoCm, Is.EqualTo(45.0m));
        }

        [Test]
        public void Constructor_PesoInvalido_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoFisico(
                    id: 10,
                    nombre: "Teclado Mecánico",
                    descripcion: "Teclado mecánico con iluminación RGB.",
                    precio: 350000m,
                    stock: 20,
                    pesoKg: 0m,
                    altoCm: 4.5m,
                    anchoCm: 18.0m,
                    largoCm: 45.0m
                ));

            Assert.That(ex!.Message, Does.Contain("peso"));
        }

        [Test]
        public void Constructor_AltoInvalido_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoFisico(
                    id: 10,
                    nombre: "Teclado Mecánico",
                    descripcion: "Teclado mecánico con iluminación RGB.",
                    precio: 350000m,
                    stock: 20,
                    pesoKg: 1.2m,
                    altoCm: -1m,
                    anchoCm: 18.0m,
                    largoCm: 45.0m
                ));

            Assert.That(ex!.Message, Does.Contain("alto").IgnoreCase);
        }

        [Test]
        public void Constructor_AnchoInvalido_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoFisico(
                    id: 10,
                    nombre: "Teclado Mecánico",
                    descripcion: "Teclado mecánico con iluminación RGB.",
                    precio: 350000m,
                    stock: 20,
                    pesoKg: 1.2m,
                    altoCm: 4.5m,
                    anchoCm: 0m,
                    largoCm: 45.0m
                ));

            Assert.That(ex!.Message, Does.Contain("ancho").IgnoreCase);
        }

        [Test]
        public void Constructor_LargoInvalido_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoFisico(
                    id: 10,
                    nombre: "Teclado Mecánico",
                    descripcion: "Teclado mecánico con iluminación RGB.",
                    precio: 350000m,
                    stock: 20,
                    pesoKg: 1.2m,
                    altoCm: 4.5m,
                    anchoCm: 18.0m,
                    largoCm: 0m
                ));

            Assert.That(ex!.Message, Does.Contain("largo").IgnoreCase);
        }

        #endregion

        #region Pruebas de actualización

        [Test]
        public void ActualizarInformacionFisica_DatosValidos_ActualizaCorrectamente()
        {
            // Arrange
            var producto = CrearProductoFisicoValido();

            // Act
            producto.ActualizarInformacionFisica(
                pesoKg: 2.5m,
                altoCm: 10.0m,
                anchoCm: 20.0m,
                largoCm: 50.0m
            );

            // Assert
            Assert.That(producto.PesoKg, Is.EqualTo(2.5m));
            Assert.That(producto.AltoCm, Is.EqualTo(10.0m));
            Assert.That(producto.AnchoCm, Is.EqualTo(20.0m));
            Assert.That(producto.LargoCm, Is.EqualTo(50.0m));
        }

        [Test]
        public void ActualizarInformacionFisica_PesoInvalido_LanzaProductException()
        {
            // Arrange
            var producto = CrearProductoFisicoValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                producto.ActualizarInformacionFisica(
                    pesoKg: -2m,
                    altoCm: 10.0m,
                    anchoCm: 20.0m,
                    largoCm: 50.0m
                ));

            Assert.That(ex!.Message, Does.Contain("peso"));
        }

        #endregion

        #region Pruebas de comportamiento

        [Test]
        public void VolumenCm3_DimensionesValidas_CalculaVolumenCorrectamente()
        {
            // Arrange
            var producto = CrearProductoFisicoValido();

            // Act
            var volumen = producto.VolumenCm3;

            // Assert
            Assert.That(volumen, Is.EqualTo(4.5m * 18.0m * 45.0m));
        }

        [Test]
        public void EsVoluminoso_VolumenMayorA100000_RetornaTrue()
        {
            // Arrange
            var producto = new ProductoFisico(
                id: 11,
                nombre: "Armario Metálico",
                descripcion: "Armario de almacenamiento industrial.",
                precio: 1500000m,
                stock: 5,
                pesoKg: 80m,
                altoCm: 200m,
                anchoCm: 100m,
                largoCm: 80m
            );

            // Act & Assert
            Assert.That(producto.EsVoluminoso(), Is.True);
        }

        [Test]
        public void EsVoluminoso_VolumenMenorOIgualA100000_RetornaFalse()
        {
            // Arrange
            var producto = CrearProductoFisicoValido();

            // Act & Assert
            Assert.That(producto.EsVoluminoso(), Is.False);
        }

        [Test]
        public void ObtenerDescripcionDetallada_ProductoValido_ContienePesoDimensionesYVolumen()
        {
            // Arrange
            var producto = CrearProductoFisicoValido();

            // Act
            var descripcion = producto.ObtenerDescripcionDetallada();

            // Assert
            Assert.That(descripcion, Does.Contain("Peso"));
            Assert.That(descripcion, Does.Contain("Dimensiones"));
            Assert.That(descripcion, Does.Contain("Volumen"));
        }

        #endregion
    }
}