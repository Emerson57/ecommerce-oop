using NUnit.Framework;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Infrastructure.Configurations;

namespace PlataformaECommerce.Tests.Infrastructure.Settings
{
    [TestFixture]
    public class ConfiguracionSistemaTests
    {
        #region Preparación

        /// Restablece un estado conocido del Singleton antes de cada prueba
        /// para evitar interferencia entre casos de prueba.
        [SetUp]
        public void Setup()
        {
            var config = ConfiguracionSistema.Instancia;

            config.ActualizarNombreSistema("Plataforma E-Commerce");
            config.ActualizarMoneda("COP");
            config.ActualizarPorcentajeImpuesto(0.19m);
            config.ActualizarMaximoProductosPorCarrito(20);
            config.ActualizarCorreoSoporte("soporte@plataformaecommerce.com");
            config.ActualizarTemaVisual("Claro");
            config.CambiarProveedorBaseDatos("SQLServer");
            config.DesactivarModoMantenimiento();
        }

        #endregion

        #region Pruebas del patrón Singleton

        [Test]
        public void Instancia_DosAccesosDistintos_RetornaLaMismaReferencia()
        {
            // Arrange
            var instanciaA = ConfiguracionSistema.Instancia;
            var instanciaB = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.That(ReferenceEquals(instanciaA, instanciaB), Is.True);
        }

        [Test]
        public void Instancia_CambioEnUnaReferencia_SeReflejaEnLaOtra()
        {
            // Arrange
            var instanciaA = ConfiguracionSistema.Instancia;
            var instanciaB = ConfiguracionSistema.Instancia;

            // Act
            instanciaA.ActualizarNombreSistema("TechMarket Pro");

            // Assert
            Assert.That(instanciaB.NombreSistema, Is.EqualTo("TechMarket Pro"));
        }

        #endregion

        #region Pruebas de actualización de configuración

        [Test]
        public void ActualizarNombreSistema_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.ActualizarNombreSistema("Mi Plataforma SaaS");

            // Assert
            Assert.That(config.NombreSistema, Is.EqualTo("Mi Plataforma SaaS"));
        }

        [Test]
        public void ActualizarMoneda_ValorValido_NormalizaYActualizaCorrectamente()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.ActualizarMoneda("usd");

            // Assert
            Assert.That(config.MonedaPorDefecto, Is.EqualTo("USD"));
        }

        [Test]
        public void ActualizarPorcentajeImpuesto_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.ActualizarPorcentajeImpuesto(0.15m);

            // Assert
            Assert.That(config.PorcentajeImpuesto, Is.EqualTo(0.15m));
        }

        [Test]
        public void ActualizarMaximoProductosPorCarrito_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.ActualizarMaximoProductosPorCarrito(50);

            // Assert
            Assert.That(config.MaximoProductosPorCarrito, Is.EqualTo(50));
        }

        [Test]
        public void ActualizarCorreoSoporte_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.ActualizarCorreoSoporte("nuevo-soporte@email.com");

            // Assert
            Assert.That(config.CorreoSoporte, Is.EqualTo("nuevo-soporte@email.com"));
        }

        [Test]
        public void ActualizarTemaVisual_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.ActualizarTemaVisual("Oscuro");

            // Assert
            Assert.That(config.TemaVisual, Is.EqualTo("Oscuro"));
        }

        [Test]
        public void CambiarProveedorBaseDatos_ValorValido_ActualizaCorrectamente()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.CambiarProveedorBaseDatos("PostgreSQL");

            // Assert
            Assert.That(config.ProveedorBaseDatos, Is.EqualTo("PostgreSQL"));
        }

        #endregion

        #region Pruebas de modo mantenimiento

        [Test]
        public void ActivarModoMantenimiento_CambiaEstadoAActivo()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            config.ActivarModoMantenimiento();

            // Assert
            Assert.That(config.ModoMantenimiento, Is.True);
        }

        [Test]
        public void DesactivarModoMantenimiento_CambiaEstadoAInactivo()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;
            config.ActivarModoMantenimiento();

            // Act
            config.DesactivarModoMantenimiento();

            // Assert
            Assert.That(config.ModoMantenimiento, Is.False);
        }

        #endregion

        #region Pruebas de errores de configuración

        [Test]
        public void ActualizarNombreSistema_ValorVacio_LanzaConfiguracionInvalidaException()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.Throws<ConfiguracionInvalidaException>(() =>
                config.ActualizarNombreSistema(""));
        }

        [Test]
        public void ActualizarMoneda_ValorVacio_LanzaConfiguracionInvalidaException()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.Throws<ConfiguracionInvalidaException>(() =>
                config.ActualizarMoneda(""));
        }

        [Test]
        public void ActualizarPorcentajeImpuesto_FueraDeRango_LanzaConfiguracionInvalidaException()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.Throws<ConfiguracionInvalidaException>(() =>
                config.ActualizarPorcentajeImpuesto(1.5m));
        }

        [Test]
        public void ActualizarMaximoProductosPorCarrito_FueraDeRango_LanzaConfiguracionInvalidaException()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.Throws<ConfiguracionInvalidaException>(() =>
                config.ActualizarMaximoProductosPorCarrito(0));
        }

        [Test]
        public void ActualizarCorreoSoporte_FormatoInvalido_LanzaConfiguracionInvalidaException()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.Throws<ConfiguracionInvalidaException>(() =>
                config.ActualizarCorreoSoporte("correo-invalido"));
        }

        [Test]
        public void ActualizarTemaVisual_ValorVacio_LanzaConfiguracionInvalidaException()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.Throws<ConfiguracionInvalidaException>(() =>
                config.ActualizarTemaVisual(""));
        }

        [Test]
        public void CambiarProveedorBaseDatos_ValorVacio_LanzaConfiguracionInvalidaException()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act & Assert
            Assert.Throws<ConfiguracionInvalidaException>(() =>
                config.CambiarProveedorBaseDatos(""));
        }

        #endregion

        #region Pruebas de representación

        [Test]
        public void ObtenerResumenConfiguracion_ConfiguracionValida_RetornaTextoConCamposPrincipales()
        {
            // Arrange
            var config = ConfiguracionSistema.Instancia;

            // Act
            var resumen = config.ObtenerResumenConfiguracion();

            // Assert
            Assert.That(resumen, Does.Contain("CONFIGURACIÓN DEL SISTEMA"));
            Assert.That(resumen, Does.Contain("Nombre del sistema"));
            Assert.That(resumen, Does.Contain("Moneda por defecto"));
            Assert.That(resumen, Does.Contain("Proveedor de base de datos"));
        }

        #endregion
    }
}