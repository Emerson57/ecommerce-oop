using NUnit.Framework;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Application.Products
{
    [TestFixture]
    public class ProductoDigitalTests
    {
        #region Método auxiliar

        /// Crea una instancia válida de ProductoDigital para reutilizar en las pruebas.
        private static ProductoDigital CrearProductoDigitalValido()
        {
            return new ProductoDigital(
                id: 1,
                nombre: "Curso C# Avanzado",
                descripcion: "Curso en video con contenido técnico avanzado.",
                precio: 120000m,
                stock: 100,
                formatoArchivo: "MP4",
                tamanoMB: 850.75m
            );
        }

        #endregion

        #region Pruebas de creación

        [Test]
        public void Constructor_DatosValidos_CreaProductoDigitalCorrectamente()
        {
            // Arrange & Act
            var producto = CrearProductoDigitalValido();

            // Assert
            Assert.That(producto.Id, Is.EqualTo(1));
            Assert.That(producto.Nombre, Is.EqualTo("Curso C# Avanzado"));
            Assert.That(producto.FormatoArchivo, Is.EqualTo("MP4"));
            Assert.That(producto.TamanoMB, Is.EqualTo(850.75m));
            Assert.That(producto.Activo, Is.True);
        }

        [Test]
        public void Constructor_FormatoArchivoVacio_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoDigital(
                    id: 1,
                    nombre: "Curso C# Avanzado",
                    descripcion: "Curso en video con contenido técnico avanzado.",
                    precio: 120000m,
                    stock: 100,
                    formatoArchivo: "",
                    tamanoMB: 850.75m
                ));

            Assert.That(ex!.Message, Does.Contain("formato"));
        }

        [Test]
        public void Constructor_FormatoArchivoMuyLargo_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoDigital(
                    id: 1,
                    nombre: "Curso C# Avanzado",
                    descripcion: "Curso en video con contenido técnico avanzado.",
                    precio: 120000m,
                    stock: 100,
                    formatoArchivo: "FORMATO_DE_ARCHIVO_EXCESIVAMENTE_LARGO",
                    tamanoMB: 850.75m
                ));

            Assert.That(ex!.Message, Does.Contain("formato"));
        }

        [Test]
        public void Constructor_TamanoInvalido_LanzaProductException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                new ProductoDigital(
                    id: 1,
                    nombre: "Curso C# Avanzado",
                    descripcion: "Curso en video con contenido técnico avanzado.",
                    precio: 120000m,
                    stock: 100,
                    formatoArchivo: "MP4",
                    tamanoMB: 0m
                ));

            Assert.That(ex!.Message, Does.Contain("tamaño"));
        }

        #endregion

        #region Pruebas de actualización

        [Test]
        public void ActualizarInformacionDigital_DatosValidos_ActualizaCorrectamente()
        {
            // Arrange
            var producto = CrearProductoDigitalValido();

            // Act
            producto.ActualizarInformacionDigital("PDF", 25.40m);

            // Assert
            Assert.That(producto.FormatoArchivo, Is.EqualTo("PDF"));
            Assert.That(producto.TamanoMB, Is.EqualTo(25.40m));
        }

        [Test]
        public void ActualizarInformacionDigital_FormatoInvalido_LanzaProductException()
        {
            // Arrange
            var producto = CrearProductoDigitalValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                producto.ActualizarInformacionDigital("", 25.40m));

            Assert.That(ex!.Message, Does.Contain("formato"));
        }

        [Test]
        public void ActualizarInformacionDigital_TamanoInvalido_LanzaProductException()
        {
            // Arrange
            var producto = CrearProductoDigitalValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                producto.ActualizarInformacionDigital("PDF", -10m));

            Assert.That(ex!.Message, Does.Contain("tamaño"));
        }

        #endregion

        #region Pruebas de comportamiento

        [Test]
        public void EsArchivoLiviano_TamanoMenorOIgualA100_RetornaTrue()
        {
            // Arrange
            var producto = new ProductoDigital(
                id: 2,
                nombre: "Manual de Usuario",
                descripcion: "Documento técnico en PDF.",
                precio: 20000m,
                stock: 50,
                formatoArchivo: "PDF",
                tamanoMB: 80m
            );

            // Act & Assert
            Assert.That(producto.EsArchivoLiviano(), Is.True);
        }

        [Test]
        public void EsArchivoLiviano_TamanoMayorA100_RetornaFalse()
        {
            // Arrange
            var producto = CrearProductoDigitalValido();

            // Act & Assert
            Assert.That(producto.EsArchivoLiviano(), Is.False);
        }

        [Test]
        public void ObtenerDescripcionDetallada_ProductoValido_ContieneFormatoYTamano()
        {
            // Arrange
            var producto = CrearProductoDigitalValido();

            // Act
            var descripcion = producto.ObtenerDescripcionDetallada();

            // Assert
            Assert.That(descripcion, Does.Contain("Formato"));
            Assert.That(descripcion, Does.Contain("MP4"));
            Assert.That(descripcion, Does.Contain("Tamaño"));
        }

        #endregion
    }
}