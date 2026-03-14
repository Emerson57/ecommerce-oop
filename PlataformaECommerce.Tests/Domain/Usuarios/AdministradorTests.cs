using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Domain.Usuarios
{
    [TestFixture]
    public class AdministradorTests
    {
        #region Métodos auxiliares

        /// Crea una instancia válida de Administrador para reutilizar en las pruebas.
        private static Administrador CrearAdministradorValido()
        {
            return new Administrador(
                id: 20,
                nombre: "Admin Principal",
                correo: "admin@email.com",
                contrasena: "Admin123",
                area: "Inventario"
            );
        }

        /// Crea un producto físico válido para pruebas de administración.
        private static ProductoFisico CrearProductoValido()
        {
            return new ProductoFisico(
                id: 200,
                nombre: "Mouse Gamer",
                descripcion: "Mouse con DPI ajustable.",
                precio: 95000m,
                stock: 10,
                pesoKg: 0.2m,
                altoCm: 4m,
                anchoCm: 7m,
                largoCm: 12m
            );
        }

        #endregion

        #region Pruebas de creación y área

        [Test]
        public void Constructor_DatosValidos_CreaAdministradorCorrectamente()
        {
            // Arrange & Act
            var administrador = CrearAdministradorValido();

            // Assert
            Assert.That(administrador.Id, Is.EqualTo(20));
            Assert.That(administrador.Nombre, Is.EqualTo("Admin Principal"));
            Assert.That(administrador.Area, Is.EqualTo("Inventario"));
            Assert.That(administrador.ObtenerRol(), Is.EqualTo("Administrador"));
        }

        [Test]
        public void Constructor_AreaInvalida_LanzaUsuarioNoValidoException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                new Administrador(
                    id: 20,
                    nombre: "Admin Principal",
                    correo: "admin@email.com",
                    contrasena: "Admin123",
                    area: ""
                ));

            Assert.That(ex!.Message, Does.Contain("área"));
        }

        [Test]
        public void ActualizarArea_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var administrador = CrearAdministradorValido();

            // Act
            administrador.ActualizarArea("Tecnología");

            // Assert
            Assert.That(administrador.Area, Is.EqualTo("Tecnología"));
        }

        [Test]
        public void ActualizarArea_ValorInvalido_LanzaUsuarioNoValidoException()
        {
            // Arrange
            var administrador = CrearAdministradorValido();

            // Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                administrador.ActualizarArea(""));

            Assert.That(ex!.Message, Does.Contain("área"));
        }

        #endregion

        #region Pruebas de inventario

        [Test]
        public void GestionarInventario_NuevoStockMayor_ReponerStockCorrectamente()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();

            // Act
            administrador.GestionarInventario(producto, 20);

            // Assert
            Assert.That(producto.Stock, Is.EqualTo(20));
        }

        [Test]
        public void GestionarInventario_NuevoStockMenor_ReduceStockCorrectamente()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();

            // Act
            administrador.GestionarInventario(producto, 5);

            // Assert
            Assert.That(producto.Stock, Is.EqualTo(5));
        }

        [Test]
        public void GestionarInventario_ProductoNulo_LanzaArgumentNullException()
        {
            // Arrange
            var administrador = CrearAdministradorValido();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                administrador.GestionarInventario(null!, 10));
        }

        [Test]
        public void GestionarInventario_StockNegativo_LanzaProductException()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                administrador.GestionarInventario(producto, -1));

            Assert.That(ex!.Message, Does.Contain("stock"));
        }

        #endregion

        #region Pruebas de promociones

        [Test]
        public void EstablecerPromocion_DescuentoValido_ActualizaPrecioCorrectamente()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();

            // Act
            administrador.EstablecerPromocion(producto, 10m);

            // Assert
            Assert.That(producto.Precio, Is.EqualTo(85500m));
        }

        [Test]
        public void EstablecerPromocion_DescuentoInvalido_LanzaProductException()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();

            // Act & Assert
            var ex = Assert.Throws<ProductException>(() =>
                administrador.EstablecerPromocion(producto, 0m));

            Assert.That(ex!.Message, Does.Contain("descuento"));
        }

        [Test]
        public void EstablecerPromocion_ProductoNoDisponible_LanzaProductoNoDisponibleException()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();
            producto.Desactivar();

            // Act & Assert
            Assert.Throws<ProductoNoDisponibleException>(() =>
                administrador.EstablecerPromocion(producto, 10m));
        }

        #endregion

        #region Pruebas de activación y desactivación de productos

        [Test]
        public void DesactivarProducto_ProductoActivo_CambiaEstadoAInactivo()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();

            // Act
            administrador.DesactivarProducto(producto);

            // Assert
            Assert.That(producto.Activo, Is.False);
        }

        [Test]
        public void ActivarProducto_ProductoInactivo_CambiaEstadoAActivo()
        {
            // Arrange
            var administrador = CrearAdministradorValido();
            var producto = CrearProductoValido();
            producto.Desactivar();

            // Act
            administrador.ActivarProducto(producto);

            // Assert
            Assert.That(producto.Activo, Is.True);
        }

        #endregion

        #region Pruebas de perfil

        [Test]
        public void MostrarPerfil_AdministradorValido_ContieneArea()
        {
            // Arrange
            var administrador = CrearAdministradorValido();

            // Act
            var perfil = administrador.MostrarPerfil();

            // Assert
            Assert.That(perfil, Does.Contain("Área"));
            Assert.That(perfil, Does.Contain("Inventario"));
        }

        #endregion
    }
}