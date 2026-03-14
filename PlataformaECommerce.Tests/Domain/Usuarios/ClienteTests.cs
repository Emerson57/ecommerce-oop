using NUnit.Framework;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Domain.Usuarios
{
    [TestFixture]
    public class ClienteTests
    {
        #region Método auxiliar

        /// Crea una instancia válida de Cliente para reutilizar en las pruebas.
        private static Cliente CrearClienteValido()
        {
            return new Cliente(
                id: 10,
                nombre: "Laura Gómez",
                correo: "laura@email.com",
                contrasena: "ClaveSegura1"
            );
        }

        #endregion

        #region Pruebas de historial de compras

        [Test]
        public void AgregarCompra_IdValido_RegistraCompraCorrectamente()
        {
            // Arrange
            var cliente = CrearClienteValido();

            // Act
            cliente.AgregarCompra(1001);

            // Assert
            Assert.That(cliente.TotalCompras, Is.EqualTo(1));
            Assert.That(cliente.TieneCompraRegistrada(1001), Is.True);
        }

        [Test]
        public void AgregarCompra_IdInvalido_LanzaUserException()
        {
            // Arrange
            var cliente = CrearClienteValido();

            // Act & Assert
            var ex = Assert.Throws<UserException>(() => cliente.AgregarCompra(0));

            Assert.That(ex!.Message, Does.Contain("pedido"));
        }

        [Test]
        public void AgregarCompra_Duplicada_LanzaUserException()
        {
            // Arrange
            var cliente = CrearClienteValido();
            cliente.AgregarCompra(1001);

            // Act & Assert
            var ex = Assert.Throws<UserException>(() => cliente.AgregarCompra(1001));

            Assert.That(ex!.Message, Does.Contain("ya se encuentra registrado"));
        }

        [Test]
        public void VerHistorial_SinCompras_RetornaMensajeDeHistorialVacio()
        {
            // Arrange
            var cliente = CrearClienteValido();

            // Act
            var historial = cliente.VerHistorial();

            // Assert
            Assert.That(historial, Does.Contain("Historial vacío"));
        }

        [Test]
        public void VerHistorial_ConCompras_RetornaListadoCorrecto()
        {
            // Arrange
            var cliente = CrearClienteValido();
            cliente.AgregarCompra(1001);
            cliente.AgregarCompra(1002);

            // Act
            var historial = cliente.VerHistorial();

            // Assert
            Assert.That(historial, Does.Contain("1001"));
            Assert.That(historial, Does.Contain("1002"));
        }

        #endregion

        #region Pruebas de preferencias

        [Test]
        public void AgregarPreferencia_ValorValido_SeAgregaCorrectamente()
        {
            // Arrange
            var cliente = CrearClienteValido();

            // Act
            cliente.AgregarPreferencia("Tecnología");

            // Assert
            Assert.That(cliente.TienePreferencia("Tecnología"), Is.True);
            Assert.That(cliente.Preferencias.Count, Is.EqualTo(1));
        }

        [Test]
        public void AgregarPreferencia_Duplicada_LanzaUserException()
        {
            // Arrange
            var cliente = CrearClienteValido();
            cliente.AgregarPreferencia("Gaming");

            // Act & Assert
            var ex = Assert.Throws<UserException>(() => cliente.AgregarPreferencia("Gaming"));

            Assert.That(ex!.Message, Does.Contain("ya existe"));
        }

        [Test]
        public void AgregarPreferencia_ValorInvalido_LanzaUserException()
        {
            // Arrange
            var cliente = CrearClienteValido();

            // Act & Assert
            var ex = Assert.Throws<UserException>(() => cliente.AgregarPreferencia(""));

            Assert.That(ex!.Message, Does.Contain("preferencia"));
        }

        [Test]
        public void EliminarPreferencia_Existente_SeEliminaCorrectamente()
        {
            // Arrange
            var cliente = CrearClienteValido();
            cliente.AgregarPreferencia("Tecnología");

            // Act
            cliente.EliminarPreferencia("Tecnología");

            // Assert
            Assert.That(cliente.TienePreferencia("Tecnología"), Is.False);
        }

        [Test]
        public void EliminarPreferencia_NoExistente_LanzaUserException()
        {
            // Arrange
            var cliente = CrearClienteValido();

            // Act & Assert
            var ex = Assert.Throws<UserException>(() => cliente.EliminarPreferencia("Gaming"));

            Assert.That(ex!.Message, Does.Contain("no existe"));
        }

        [Test]
        public void LimpiarPreferencias_ConDatos_EliminaTodasLasPreferencias()
        {
            // Arrange
            var cliente = CrearClienteValido();
            cliente.AgregarPreferencia("Gaming");
            cliente.AgregarPreferencia("Tecnología");

            // Act
            cliente.LimpiarPreferencias();

            // Assert
            Assert.That(cliente.Preferencias.Count, Is.EqualTo(0));
        }

        #endregion

        #region Pruebas de comportamiento general

        [Test]
        public void ObtenerRol_ClienteValido_RetornaCliente()
        {
            // Arrange
            var cliente = CrearClienteValido();

            // Act & Assert
            Assert.That(cliente.ObtenerRol(), Is.EqualTo("Cliente"));
        }

        [Test]
        public void MostrarPerfil_ClienteValido_ContieneComprasYPreferencias()
        {
            // Arrange
            var cliente = CrearClienteValido();
            cliente.AgregarCompra(1001);
            cliente.AgregarPreferencia("Tecnología");

            // Act
            var perfil = cliente.MostrarPerfil();

            // Assert
            Assert.That(perfil, Does.Contain("Compras"));
            Assert.That(perfil, Does.Contain("Preferencias"));
            Assert.That(perfil, Does.Contain("Tecnología"));
        }

        #endregion
    }
}