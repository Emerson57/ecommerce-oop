using System;
using System.Collections.Generic;
using System.Linq;

namespace PlataformaECommerce.Domain.Entities
{
    public sealed class Cliente : Usuario
    {
        #region Constantes de negocio

        /// Longitud mínima permitida para una preferencia.
        private const int LongitudMinimaPreferencia = 2;

        /// Longitud máxima permitida para una preferencia.
        private const int LongitudMaximaPreferencia = 50;

        #endregion

        #region Campos privados

        /// Historial interno de compras del cliente.
        private readonly List<int> _historialCompras = new();

        /// Conjunto de preferencias del cliente.
        private readonly HashSet<string> _preferencias = new(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructores

        /// Constructor protegido sin parámetros.
        protected Cliente()
        {
        }

        /// Crea una nueva instancia de Cliente con los datos base del usuario.
        public Cliente(int id, string nombre, string correo, string contrasena)
            : base(id, nombre, correo, contrasena)
        {
        }

        #endregion

        #region Propiedades públicas

        /// Devuelve el historial de compras del cliente como colección de solo lectura.
        public IReadOnlyList<int> HistorialCompras => _historialCompras.AsReadOnly();

        /// Devuelve las preferencias del cliente como colección de solo lectura.
        public IReadOnlyCollection<string> Preferencias => _preferencias;

        /// Cantidad total de compras registradas en el historial.
        public int TotalCompras => _historialCompras.Count;

        #endregion

        #region Métodos de negocio

        /// Registra una compra en el historial del cliente.
        public void AgregarCompra(int idPedido)
        {
            if (idPedido <= 0)
                throw new ArgumentOutOfRangeException(nameof(idPedido), "El identificador del pedido debe ser mayor que cero.");

            if (_historialCompras.Contains(idPedido))
                throw new InvalidOperationException($"El pedido con Id {idPedido} ya se encuentra registrado en el historial del cliente.");

            _historialCompras.Add(idPedido);
            ActualizarFechaModificacion();
        }

        /// Verifica si el cliente ya tiene registrada una compra específica.
        public bool TieneCompraRegistrada(int idPedido)
        {
            if (idPedido <= 0)
                return false;

            return _historialCompras.Contains(idPedido);
        }

        /// Devuelve el historial de compras en formato legible.
        public string VerHistorial()
        {
            if (_historialCompras.Count == 0)
                return "Historial vacío: el cliente aún no registra compras.";

            return $"Historial de compras (IDs de pedidos): {string.Join(", ", _historialCompras)}";
        }

        /// Agrega una nueva preferencia al perfil del cliente.
        public void AgregarPreferencia(string preferencia)
        {
            string preferenciaNormalizada = ValidarPreferencia(preferencia);

            if (!_preferencias.Add(preferenciaNormalizada))
                throw new InvalidOperationException($"La preferencia '{preferenciaNormalizada}' ya existe para este cliente.");

            ActualizarFechaModificacion();
        }

        /// Elimina una preferencia del cliente.
        public void EliminarPreferencia(string preferencia)
        {
            string preferenciaNormalizada = ValidarPreferencia(preferencia);

            if (!_preferencias.Remove(preferenciaNormalizada))
                throw new InvalidOperationException($"La preferencia '{preferenciaNormalizada}' no existe en el perfil del cliente.");

            ActualizarFechaModificacion();
        }

        /// Verifica si el cliente tiene registrada una preferencia específica.
        public bool TienePreferencia(string preferencia)
        {
            if (string.IsNullOrWhiteSpace(preferencia))
                return false;

            string preferenciaNormalizada = preferencia.Trim();
            return _preferencias.Contains(preferenciaNormalizada);
        }

        /// Limpia todas las preferencias registradas del cliente.
        public void LimpiarPreferencias()
        {
            if (_preferencias.Count == 0)
                return;

            _preferencias.Clear();
            ActualizarFechaModificacion();
        }

        #endregion

        #region Overrides

        /// Devuelve el rol específico del usuario dentro del sistema.
        public override string ObtenerRol()
        {
            return "Cliente";
        }

        /// Devuelve una representación legible del perfil del cliente,
        /// incluyendo información adicional relevante para su contexto.
        public override string MostrarPerfil()
        {
            string preferenciasTexto = _preferencias.Count == 0
                ? "Sin preferencias"
                : string.Join(", ", _preferencias.OrderBy(p => p));

            return $"{base.MostrarPerfil()} | Compras: {TotalCompras} | Preferencias: {preferenciasTexto}";
        }

        /// Devuelve una representación corta del cliente.
        public override string ToString()
        {
            return $"{Nombre} ({Correo}) - Cliente | Compras: {TotalCompras}";
        }

        #endregion

        #region Métodos privados de validación

        /// Valida y normaliza una preferencia del cliente.
        private static string ValidarPreferencia(string preferencia)
        {
            if (string.IsNullOrWhiteSpace(preferencia))
                throw new ArgumentException("La preferencia es obligatoria.", nameof(preferencia));

            string preferenciaNormalizada = preferencia.Trim();

            if (preferenciaNormalizada.Length < LongitudMinimaPreferencia)
                throw new ArgumentException($"La preferencia debe tener al menos {LongitudMinimaPreferencia} caracteres.", nameof(preferencia));

            if (preferenciaNormalizada.Length > LongitudMaximaPreferencia)
                throw new ArgumentException($"La preferencia no puede superar los {LongitudMaximaPreferencia} caracteres.", nameof(preferencia));

            return preferenciaNormalizada;
        }

        #endregion
    }
}