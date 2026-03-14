using System;
using System.Text;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Infrastructure.Settings
{
    public sealed class ConfiguracionSistema
    {
        #region Singleton

        private static readonly Lazy<ConfiguracionSistema> _instancia =
            new Lazy<ConfiguracionSistema>(() => new ConfiguracionSistema());

        /// Instancia única del sistema de configuración.
        public static ConfiguracionSistema Instancia => _instancia.Value;

        #endregion

        #region Constantes de negocio

        private const decimal ImpuestoMinimo = 0m;
        private const decimal ImpuestoMaximo = 1m;

        private const int MaximoProductosCarritoMin = 1;
        private const int MaximoProductosCarritoMax = 1000;

        #endregion

        #region Propiedades públicas

        public string NombreSistema { get; private set; }
        public string MonedaPorDefecto { get; private set; }
        public decimal PorcentajeImpuesto { get; private set; }
        public int MaximoProductosPorCarrito { get; private set; }
        public string CorreoSoporte { get; private set; }
        public string TemaVisual { get; private set; }
        public bool ModoMantenimiento { get; private set; }
        public string ProveedorBaseDatos { get; private set; }

        #endregion

        #region Constructor

        private ConfiguracionSistema()
        {
            NombreSistema = "Plataforma E-Commerce";
            MonedaPorDefecto = "COP";
            PorcentajeImpuesto = 0.19m;
            MaximoProductosPorCarrito = 20;
            CorreoSoporte = "soporte@plataformaecommerce.com";
            TemaVisual = "Claro";
            ModoMantenimiento = false;
            ProveedorBaseDatos = "SQLServer";
        }

        #endregion

        #region Métodos de configuración

        public void ActualizarNombreSistema(string nombreSistema)
        {
            NombreSistema = ValidarTextoObligatorio(nombreSistema, "El nombre del sistema no puede estar vacío.");
        }

        public void ActualizarMoneda(string moneda)
        {
            string monedaNormalizada = ValidarTextoObligatorio(moneda, "La moneda no puede estar vacía");
            MonedaPorDefecto = monedaNormalizada.ToUpperInvariant();
        }

        public void ActualizarPorcentajeImpuesto(decimal porcentajeImpuesto)
        {
            if (porcentajeImpuesto < ImpuestoMinimo || porcentajeImpuesto > ImpuestoMaximo)
                throw new ConfiguracionInvalidaException("El porcentaje de impuesto debe estar entre 0 y 1.");

            PorcentajeImpuesto = decimal.Round(porcentajeImpuesto, 4, MidpointRounding.AwayFromZero);
        }

        public void ActualizarMaximoProductosPorCarrito(int maximoProductos)
        {
            if (maximoProductos < MaximoProductosCarritoMin || maximoProductos > MaximoProductosCarritoMax)
                throw new ConfiguracionInvalidaException(
                    $"El máximo de productos por carrito debe estar entre {MaximoProductosCarritoMin} y {MaximoProductosCarritoMax}."
                );

            MaximoProductosPorCarrito = maximoProductos;
        }

        public void ActualizarCorreoSoporte(string correoSoporte)
        {
            string correoNormalizado = ValidarTextoObligatorio(correoSoporte, "El correo de soporte no puede estar vacío.");

            if (!correoNormalizado.Contains("@"))
                throw new ConfiguracionInvalidaException("El correo de soporte no tiene un formato válido.");

            CorreoSoporte = correoNormalizado;
        }

        public void ActualizarTemaVisual(string temaVisual)
        {
            TemaVisual = ValidarTextoObligatorio(temaVisual, "El tema visual no puede estar vacío.");
        }

        public void CambiarProveedorBaseDatos(string proveedorBaseDatos)
        {
            ProveedorBaseDatos = ValidarTextoObligatorio(
                proveedorBaseDatos,
                "El proveedor de base de datos no puede estar vacío."
            );
        }

        public void ActivarModoMantenimiento()
        {
            ModoMantenimiento = true;
        }

        public void DesactivarModoMantenimiento()
        {
            ModoMantenimiento = false;
        }

        #endregion

        #region Métodos auxiliares

        /// Valida texto obligatorio del sistema.
        private static string ValidarTextoObligatorio(string valor, string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ConfiguracionInvalidaException(mensajeError);

            return valor.Trim();
        }

        #endregion

        #region Representación

        public string ObtenerResumenConfiguracion()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== CONFIGURACIÓN DEL SISTEMA ===");
            sb.AppendLine($"Nombre del sistema: {NombreSistema}");
            sb.AppendLine($"Moneda por defecto: {MonedaPorDefecto}");
            sb.AppendLine($"Porcentaje de impuesto: {PorcentajeImpuesto:P}");
            sb.AppendLine($"Máximo de productos por carrito: {MaximoProductosPorCarrito}");
            sb.AppendLine($"Correo de soporte: {CorreoSoporte}");
            sb.AppendLine($"Tema visual: {TemaVisual}");
            sb.AppendLine($"Modo mantenimiento: {(ModoMantenimiento ? "Activo" : "Inactivo")}");
            sb.AppendLine($"Proveedor de base de datos: {ProveedorBaseDatos}");

            return sb.ToString();
        }

        #endregion
    }
}