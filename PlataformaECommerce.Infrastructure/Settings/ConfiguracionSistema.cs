using System;
using System.Text;

namespace PlataformaECommerce.Infrastructure.Settings
{
    public sealed class ConfiguracionSistema
    {
        private static readonly Lazy<ConfiguracionSistema> _instancia =
            new Lazy<ConfiguracionSistema>(() => new ConfiguracionSistema());

        public static ConfiguracionSistema Instancia => _instancia.Value;

        public string NombreSistema { get; private set; }
        public string MonedaPorDefecto { get; private set; }
        public decimal PorcentajeImpuesto { get; private set; }
        public int MaximoProductosPorCarrito { get; private set; }
        public string CorreoSoporte { get; private set; }
        public string TemaVisual { get; private set; }
        public bool ModoMantenimiento { get; private set; }
        public string ProveedorBaseDatos { get; private set; }

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

        public void ActualizarNombreSistema(string nombreSistema)
        {
            if (string.IsNullOrWhiteSpace(nombreSistema))
                throw new ArgumentException("El nombre del sistema no puede estar vacío.");

            NombreSistema = nombreSistema.Trim();
        }

        public void ActualizarMoneda(string moneda)
        {
            if (string.IsNullOrWhiteSpace(moneda))
                throw new ArgumentException("La moneda no puede estar vacía.");

            MonedaPorDefecto = moneda.Trim().ToUpper();
        }

        public void ActualizarPorcentajeImpuesto(decimal porcentajeImpuesto)
        {
            if (porcentajeImpuesto < 0 || porcentajeImpuesto > 1)
                throw new ArgumentException("El porcentaje de impuesto debe estar entre 0 y 1.");

            PorcentajeImpuesto = porcentajeImpuesto;
        }

        public void ActualizarMaximoProductosPorCarrito(int maximoProductos)
        {
            if (maximoProductos <= 0)
                throw new ArgumentException("El máximo de productos por carrito debe ser mayor que cero.");

            MaximoProductosPorCarrito = maximoProductos;
        }

        public void ActualizarCorreoSoporte(string correoSoporte)
        {
            if (string.IsNullOrWhiteSpace(correoSoporte))
                throw new ArgumentException("El correo de soporte no puede estar vacío.");

            if (!correoSoporte.Contains("@"))
                throw new ArgumentException("El correo de soporte no tiene un formato válido.");

            CorreoSoporte = correoSoporte.Trim();
        }

        public void ActualizarTemaVisual(string temaVisual)
        {
            if (string.IsNullOrWhiteSpace(temaVisual))
                throw new ArgumentException("El tema visual no puede estar vacío.");

            TemaVisual = temaVisual.Trim();
        }

        public void CambiarProveedorBaseDatos(string proveedorBaseDatos)
        {
            if (string.IsNullOrWhiteSpace(proveedorBaseDatos))
                throw new ArgumentException("El proveedor de base de datos no puede estar vacío.");

            ProveedorBaseDatos = proveedorBaseDatos.Trim();
        }

        public void ActivarModoMantenimiento()
        {
            ModoMantenimiento = true;
        }

        public void DesactivarModoMantenimiento()
        {
            ModoMantenimiento = false;
        }

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
    }
}