using System;
using System.Collections.Generic;
using System.Linq;

namespace PlataformaECommerce.Dominio
{
    public sealed class Cliente : Usuario
    {
        #region Campos privados

        /// Historial interno de compras.
        private readonly List<int> _historialCompras;

        /// Preferencias internas del cliente (ej: "Tecnología", "Gaming", "Audio").
        private readonly HashSet<string> _preferencias;

        #endregion

        #region Propiedades públicas (solo lectura)

        /// Historial de compras (solo lectura).
        public IReadOnlyList<int> HistorialCompras => _historialCompras.AsReadOnly();

        /// Preferencias del cliente (solo lectura).
        public IReadOnlyCollection<string> Preferencias => _preferencias;

        #endregion

        #region Constructor

        /// Crea un cliente inicializando los datos base del usuario y sus colecciones internas.
        public Cliente(int id, string nombre, string correo, string contrasena)
            : base(id, nombre, correo, contrasena)
        {
            _historialCompras = new List<int>();
            _preferencias = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Métodos requeridos (enunciado)

        /// Registra una compra en el historial del cliente.
        public void AgregarCompra(int idPedido)
        {
            if (idPedido <= 0)
                throw new ArgumentException("El id del pedido debe ser mayor que 0.", nameof(idPedido));

            if (_historialCompras.Contains(idPedido))
                throw new InvalidOperationException($"El pedido {idPedido} ya existe en el historial del cliente.");

            _historialCompras.Add(idPedido);
        }

        /// Devuelve el historial en formato legible.
        public string VerHistorial()
        {
            if (_historialCompras.Count == 0)
                return "Historial vacío: el cliente aún no registra compras.";

            return "Historial de compras (IDs de pedidos): " + string.Join(", ", _historialCompras);
        }

        /// Agrega una preferencia del cliente.
        public void AgregarPreferencia(string preferencia)
        {
            if (string.IsNullOrWhiteSpace(preferencia))
                throw new ArgumentException("La preferencia no puede estar vacía.", nameof(preferencia));

            var preferenciaNormalizada = preferencia.Trim();

            // HashSet evita duplicados; si no agrega, es porque ya existía.
            if (!_preferencias.Add(preferenciaNormalizada))
                throw new InvalidOperationException($"La preferencia '{preferenciaNormalizada}' ya existe.");
        }

        #endregion

        #region Overrides (polimorfismo)

        /// Rol específico del usuario en el sistema.
        public override string ObtenerRol() => "Cliente";

        /// Presenta el perfil con información adicional del cliente.
        public override string MostrarPerfil()
        {
            var prefs = _preferencias.Count == 0
                ? "Sin preferencias"
                : string.Join(", ", _preferencias);

            return $"{base.MostrarPerfil()} | Compras: {_historialCompras.Count} | Preferencias: {prefs}";
        }

        #endregion
    }
}