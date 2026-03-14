using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Domain.Usuarios
{
    [TestFixture]
    public class UsuarioTests
    {
        #region Método auxiliar

        /// Crea una instancia válida de Cliente para reutilizar
        /// en las pruebas del comportamiento base de Usuario.
        private static Cliente CrearUsuarioValido()
        {
            return new Cliente(
                id: 1,
                nombre: "Juan Pérez",
                correo: "juan@email.com",
                contrasena: "Clave123"
            );
        }

        #endregion

        #region Pruebas de creación

        [Test]
        public void Constructor_DatosValidos_CreaUsuarioCorrectamente()
        {
            // Arrange & Act
            var usuario = CrearUsuarioValido();

            // Assert
            Assert.That(usuario.Id, Is.EqualTo(1));
            Assert.That(usuario.Nombre, Is.EqualTo("Juan Pérez"));
            Assert.That(usuario.Correo, Is.EqualTo("juan@email.com"));
            Assert.That(usuario.Activo, Is.True);
        }

        [Test]
        public void Constructor_IdInvalido_LanzaUsuarioNoValidoException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                new Cliente(
                    id: 0,
                    nombre: "Juan Pérez",
                    correo: "juan@email.com",
                    contrasena: "Clave123"
                ));

            Assert.That(ex!.Message, Does.Contain("Id"));
        }

        [Test]
        public void Constructor_NombreVacio_LanzaUsuarioNoValidoException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                new Cliente(
                    id: 1,
                    nombre: "",
                    correo: "juan@email.com",
                    contrasena: "Clave123"
                ));

            Assert.That(ex!.Message, Does.Contain("nombre"));
        }

        [Test]
        public void Constructor_CorreoInvalido_LanzaUsuarioNoValidoException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                new Cliente(
                    id: 1,
                    nombre: "Juan Pérez",
                    correo: "correo-invalido",
                    contrasena: "Clave123"
                ));

            Assert.That(ex!.Message, Does.Contain("correo"));
        }

        [Test]
        public void Constructor_ContrasenaInvalida_LanzaUsuarioNoValidoException()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                new Cliente(
                    id: 1,
                    nombre: "Juan Pérez",
                    correo: "juan@email.com",
                    contrasena: "123"
                ));

            Assert.That(ex!.Message, Does.Contain("contraseña"));
        }

        #endregion

        #region Pruebas de actualización

        [Test]
        public void ActualizarDatos_DatosValidos_ActualizaCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act
            usuario.ActualizarDatos("Carlos Gómez", "carlos@email.com");

            // Assert
            Assert.That(usuario.Nombre, Is.EqualTo("Carlos Gómez"));
            Assert.That(usuario.Correo, Is.EqualTo("carlos@email.com"));
        }

        [Test]
        public void ActualizarDatos_NombreInvalido_LanzaUsuarioNoValidoException()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                usuario.ActualizarDatos("", "carlos@email.com"));

            Assert.That(ex!.Message, Does.Contain("nombre"));
        }

        [Test]
        public void ActualizarDatos_CorreoInvalido_LanzaUsuarioNoValidoException()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                usuario.ActualizarDatos("Carlos Gómez", "correo-invalido"));

            Assert.That(ex!.Message, Does.Contain("correo"));
        }

        #endregion

        #region Pruebas de contraseña

        [Test]
        public void CambiarContrasena_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act
            usuario.CambiarContrasena("NuevaClave123");

            // Assert
            Assert.That(usuario.VerificarContrasena("NuevaClave123"), Is.True);
        }

        [Test]
        public void CambiarContrasena_ValorInvalido_LanzaUsuarioNoValidoException()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act & Assert
            var ex = Assert.Throws<UsuarioNoValidoException>(() =>
                usuario.CambiarContrasena("123"));

            Assert.That(ex!.Message, Does.Contain("contraseña"));
        }

        [Test]
        public void VerificarContrasena_ClaveCorrecta_RetornaTrue()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act & Assert
            Assert.That(usuario.VerificarContrasena("Clave123"), Is.True);
        }

        [Test]
        public void VerificarContrasena_ClaveIncorrecta_RetornaFalse()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act & Assert
            Assert.That(usuario.VerificarContrasena("OtraClave"), Is.False);
        }

        #endregion

        #region Pruebas de estado

        [Test]
        public void Desactivar_UsuarioActivo_CambiaEstadoAInactivo()
        {
            // Arrange
            var usuario = CrearUsuarioValido();

            // Act
            usuario.Desactivar();

            // Assert
            Assert.That(usuario.Activo, Is.False);
        }

        [Test]
        public void Activar_UsuarioInactivo_CambiaEstadoAActivo()
        {
            // Arrange
            var usuario = CrearUsuarioValido();
            usuario.Desactivar();

            // Act
            usuario.Activar();

            // Assert
            Assert.That(usuario.Activo, Is.True);
        }

        #endregion
    }
}