using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Infrastructure.Factories;

namespace PlataformaECommerce.Tests.Infrastructure.Factories
{
    [TestFixture]
    public class FabricaEntidadesTests
    {
        #region Pruebas de creación de productos

        [Test]
        public void CrearProductoDigital_DatosValidos_CreaProductoDigitalCorrectamente()
        {
            // Arrange & Act
            var producto = FabricaEntidades.CrearProductoDigital(
                id: 1,
                nombre: "Curso C#",
                descripcion: "Curso completo en video.",
                precio: 120000m,
                stock: 50,
                formatoArchivo: "MP4",
                tamanoMB: 850.75m
            );

            // Assert
            Assert.That(producto, Is.TypeOf<ProductoDigital>());

            var productoDigital = (ProductoDigital)producto;
            Assert.That(productoDigital.Id, Is.EqualTo(1));
            Assert.That(productoDigital.Nombre, Is.EqualTo("Curso C#"));
            Assert.That(productoDigital.FormatoArchivo, Is.EqualTo("MP4"));
            Assert.That(productoDigital.TamanoMB, Is.EqualTo(850.75m));
        }

        [Test]
        public void CrearProductoFisico_DatosValidos_CreaProductoFisicoCorrectamente()
        {
            // Arrange & Act
            var producto = FabricaEntidades.CrearProductoFisico(
                id: 2,
                nombre: "Teclado Mecánico",
                descripcion: "Teclado con retroiluminación.",
                precio: 350000m,
                stock: 10,
                pesoKg: 1.2m,
                altoCm: 4.5m,
                anchoCm: 18m,
                largoCm: 45m
            );

            // Assert
            Assert.That(producto, Is.TypeOf<ProductoFisico>());

            var productoFisico = (ProductoFisico)producto;
            Assert.That(productoFisico.Id, Is.EqualTo(2));
            Assert.That(productoFisico.Nombre, Is.EqualTo("Teclado Mecánico"));
            Assert.That(productoFisico.PesoKg, Is.EqualTo(1.2m));
        }

        [Test]
        public void CrearProductoDigital_IdInvalido_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearProductoDigital(
                    id: 0,
                    nombre: "Curso C#",
                    descripcion: "Curso completo en video.",
                    precio: 120000m,
                    stock: 50,
                    formatoArchivo: "MP4",
                    tamanoMB: 850.75m
                ));

            Assert.That(ex!.Message, Does.Contain("identificador"));
        }

        [Test]
        public void CrearProductoFisico_IdInvalido_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearProductoFisico(
                    id: -1,
                    nombre: "Teclado Mecánico",
                    descripcion: "Teclado con retroiluminación.",
                    precio: 350000m,
                    stock: 10,
                    pesoKg: 1.2m,
                    altoCm: 4.5m,
                    anchoCm: 18m,
                    largoCm: 45m
                ));

            Assert.That(ex!.Message, Does.Contain("identificador"));
        }

        #endregion

        #region Pruebas de creación de usuarios

        [Test]
        public void CrearCliente_DatosValidos_CreaClienteCorrectamente()
        {
            // Arrange & Act
            var usuario = FabricaEntidades.CrearCliente(
                id: 10,
                nombre: "Laura Gómez",
                correo: "laura@email.com",
                contrasena: "Clave123"
            );

            // Assert
            Assert.That(usuario, Is.TypeOf<Cliente>());

            var cliente = (Cliente)usuario;
            Assert.That(cliente.Id, Is.EqualTo(10));
            Assert.That(cliente.Nombre, Is.EqualTo("Laura Gómez"));
            Assert.That(cliente.Correo, Is.EqualTo("laura@email.com"));
        }

        [Test]
        public void CrearAdministrador_DatosValidos_CreaAdministradorCorrectamente()
        {
            // Arrange & Act
            var usuario = FabricaEntidades.CrearAdministrador(
                id: 20,
                nombre: "Admin Principal",
                correo: "admin@email.com",
                contrasena: "Admin123",
                area: "Inventario"
            );

            // Assert
            Assert.That(usuario, Is.TypeOf<Administrador>());

            var administrador = (Administrador)usuario;
            Assert.That(administrador.Id, Is.EqualTo(20));
            Assert.That(administrador.Area, Is.EqualTo("Inventario"));
        }

        [Test]
        public void CrearCliente_IdInvalido_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearCliente(
                    id: 0,
                    nombre: "Laura Gómez",
                    correo: "laura@email.com",
                    contrasena: "Clave123"
                ));

            Assert.That(ex!.Message, Does.Contain("identificador"));
        }

        [Test]
        public void CrearAdministrador_IdInvalido_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearAdministrador(
                    id: 0,
                    nombre: "Admin Principal",
                    correo: "admin@email.com",
                    contrasena: "Admin123",
                    area: "Inventario"
                ));

            Assert.That(ex!.Message, Does.Contain("identificador"));
        }

        #endregion

        #region Pruebas del factory genérico

        [Test]
        public void CrearProductoPorTipo_TipoDigital_DatosValidos_CreaProductoDigital()
        {
            // Arrange & Act
            var producto = FabricaEntidades.CrearProductoPorTipo(
                tipoProducto: "digital",
                id: 100,
                nombre: "Ebook Arquitectura",
                descripcion: "Libro digital sobre arquitectura de software.",
                precio: 75000m,
                stock: 100,
                "PDF",
                12.5m
            );

            // Assert
            Assert.That(producto, Is.TypeOf<ProductoDigital>());
        }

        [Test]
        public void CrearProductoPorTipo_TipoFisico_DatosValidos_CreaProductoFisico()
        {
            // Arrange & Act
            var producto = FabricaEntidades.CrearProductoPorTipo(
                tipoProducto: "fisico",
                id: 101,
                nombre: "Mouse Profesional",
                descripcion: "Mouse ergonómico de precisión.",
                precio: 150000m,
                stock: 15,
                0.25m,
                4m,
                7m,
                12m
            );

            // Assert
            Assert.That(producto, Is.TypeOf<ProductoFisico>());
        }

        [Test]
        public void CrearProductoPorTipo_TipoVacio_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearProductoPorTipo(
                    tipoProducto: "",
                    id: 100,
                    nombre: "Ebook Arquitectura",
                    descripcion: "Libro digital sobre arquitectura de software.",
                    precio: 75000m,
                    stock: 100,
                    "PDF",
                    12.5m
                ));

            Assert.That(ex!.Message, Does.Contain("tipo de producto"));
        }

        [Test]
        public void CrearProductoPorTipo_TipoNoSoportado_LanzaEntidadNoSoportadaException()
        {
            // Arrange, Act & Assert
            Assert.Throws<EntidadNoSoportadaException>(() =>
                FabricaEntidades.CrearProductoPorTipo(
                    tipoProducto: "hibrido",
                    id: 100,
                    nombre: "Producto híbrido",
                    descripcion: "Descripción",
                    precio: 75000m,
                    stock: 100
                ));
        }

        [Test]
        public void CrearProductoPorTipo_DigitalSinParametrosSuficientes_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearProductoPorTipo(
                    tipoProducto: "digital",
                    id: 100,
                    nombre: "Ebook Arquitectura",
                    descripcion: "Libro digital sobre arquitectura de software.",
                    precio: 75000m,
                    stock: 100,
                    "PDF"
                ));

            Assert.That(ex!.Message, Does.Contain("ProductoDigital requiere"));
        }

        [Test]
        public void CrearProductoPorTipo_FisicoSinParametrosSuficientes_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearProductoPorTipo(
                    tipoProducto: "fisico",
                    id: 101,
                    nombre: "Mouse Profesional",
                    descripcion: "Mouse ergonómico de precisión.",
                    precio: 150000m,
                    stock: 15,
                    0.25m,
                    4m
                ));

            Assert.That(ex!.Message, Does.Contain("ProductoFisico requiere"));
        }

        [Test]
        public void CrearProductoPorTipo_DigitalConTipoParametroIncorrecto_LanzaFactoryException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<FactoryException>(() =>
                FabricaEntidades.CrearProductoPorTipo(
                    tipoProducto: "digital",
                    id: 100,
                    nombre: "Ebook Arquitectura",
                    descripcion: "Libro digital sobre arquitectura de software.",
                    precio: 75000m,
                    stock: 100,
                    123,
                    12.5m
                ));

            Assert.That(ex!.Message, Does.Contain("formatoArchivo"));
        }

        #endregion
    }
}