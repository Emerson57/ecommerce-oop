using System;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.Entities.Products
{
    public sealed class ProductoDigital : Producto
    {
        #region Constantes de negocio

        /// Longitud máxima permitida para el formato del archivo.
        private const int LongitudMaximaFormato = 20;

        /// Tamaño máximo permitido para archivos digitales (MB).
        private const decimal TamanoMaximoArchivoMB = 10240; // 10 GB

        #endregion

        #region Campos privados

        private string _formatoArchivo = string.Empty;
        private decimal _tamanoMB;

        #endregion

        #region Constructores

        /// Constructor protegido sin parámetros.
        protected ProductoDigital()
        {
        }

        /// Crea una nueva instancia de un producto digital con sus datos base
        public ProductoDigital(
            int id,
            string nombre,
            string descripcion,
            decimal precio,
            int stock,
            string formatoArchivo,
            decimal tamanoMB)
            : base(id, nombre, descripcion, precio, stock)
        {
            _formatoArchivo = ValidarFormatoArchivo(formatoArchivo);
            _tamanoMB = ValidarTamanoMB(tamanoMB);
        }

        #endregion

        #region Propiedades públicas

        /// Formato del archivo digital.
        public string FormatoArchivo => _formatoArchivo;

        /// Tamaño del archivo en megabytes (MB).
        public decimal TamanoMB => _tamanoMB;

        #endregion

        #region Métodos de negocio

        /// Actualiza la información técnica del archivo digital.
        public void ActualizarInformacionDigital(string formatoArchivo, decimal tamanoMB)
        {
            _formatoArchivo = ValidarFormatoArchivo(formatoArchivo);
            _tamanoMB = ValidarTamanoMB(tamanoMB);

            ActualizarFechaModificacion();
        }

        /// Indica si el archivo digital puede considerarse liviano según su tamaño.
        public bool EsArchivoLiviano() => _tamanoMB <= 100;

        /// Devuelve una descripción detallada del producto digital incluyendo
        /// información técnica del archivo.
        public override string ObtenerDescripcionDetallada()
        {
            return $"{base.ObtenerDescripcionDetallada()} | Formato: {FormatoArchivo} | Tamaño: {TamanoMB:0.##} MB";
        }

        #endregion

        #region Métodos privados de validación

        /// Valida el formato del archivo digital.
        private static string ValidarFormatoArchivo(string formatoArchivo)
        {
            if (string.IsNullOrWhiteSpace(formatoArchivo))
                throw new ProductException("El formato del archivo es obligatorio.");

            string formatoNormalizado = formatoArchivo.Trim().ToUpperInvariant();

            if (formatoNormalizado.Length > LongitudMaximaFormato)
                throw new ProductException($"El formato del archivo no puede superar los {LongitudMaximaFormato} caracteres.");

            return formatoNormalizado;
        }

        /// Valida que el tamaño del archivo sea válido.
        private static decimal ValidarTamanoMB(decimal tamanoMB)
        {
            if (tamanoMB <= 0)
                throw new ProductException("El tamaño del archivo debe ser mayor que cero.");

            if (tamanoMB > TamanoMaximoArchivoMB)
                throw new ProductException($"El tamaño del archivo no puede superar los {TamanoMaximoArchivoMB} MB.");

            return decimal.Round(tamanoMB, 2, MidpointRounding.AwayFromZero);
        }

        #endregion

        #region Representación textual

        /// Devuelve una representación corta del producto digital.
        public override string ToString()
        {
            return $"{base.ToString()} | Digital: {FormatoArchivo} ({TamanoMB:0.##} MB)";
        }

        #endregion
    }
}