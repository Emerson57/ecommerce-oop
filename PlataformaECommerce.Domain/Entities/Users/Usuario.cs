using System;
using System.Text.RegularExpressions;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.Entities.Users
{
    public abstract class Usuario
    {
        #region Constantes de negocio

        /// Longitud mínima permitida para el nombre del usuario.
        private const int LongitudMinimaNombre = 3;

        /// Longitud máxima permitida para el nombre del usuario.
        private const int LongitudMaximaNombre = 100;

        /// Longitud mínima requerida para la contraseña.
        private const int LongitudMinimaContrasena = 6;

        /// Patrón básico para validar correos electrónicos.
        private const string PatronCorreo = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        #endregion

        #region Campos privados

        private int _id;
        private string _nombre = string.Empty;
        private string _correo = string.Empty;
        private string _contrasena = string.Empty;
        private bool _activo;
        private DateTime _fechaCreacion;
        private DateTime _fechaActualizacion;

        #endregion

        #region Constructores

        /// Constructor protegido sin parámetros.
        protected Usuario()
        {
        }

        /// Constructor principal de la entidad Usuario.
        protected Usuario(int id, string nombre, string correo, string contrasena)
        {
            _id = ValidarId(id);
            _nombre = ValidarNombre(nombre);
            _correo = ValidarCorreo(correo);
            _contrasena = ValidarContrasena(contrasena);
            _activo = true;
            _fechaCreacion = DateTime.UtcNow;
            _fechaActualizacion = DateTime.UtcNow;
        }

        #endregion

        #region Propiedades públicas

        /// Identificador único del usuario.
        public int Id => _id;

        /// Nombre completo del usuario.
        public string Nombre => _nombre;

        /// Correo electrónico del usuario.
        public string Correo => _correo;

        /// Indica si el usuario está activo en el sistema.
        public bool Activo => _activo;

        /// Fecha de creación del usuario en el sistema.
        public DateTime FechaCreacion => _fechaCreacion;

        /// Fecha de la última actualización del usuario.
        public DateTime FechaActualizacion => _fechaActualizacion;

        #endregion

        #region Métodos de negocio

        /// Permite actualizar los datos básicos del usuario.
        public virtual void ActualizarDatos(string nombre, string correo)
        {
            _nombre = ValidarNombre(nombre);
            _correo = ValidarCorreo(correo);
            ActualizarFechaModificacion();
        }

        /// Permite cambiar la contraseña del usuario de forma controlada.
        public void CambiarContrasena(string nuevaContrasena)
        {
            _contrasena = ValidarContrasena(nuevaContrasena);
            ActualizarFechaModificacion();
        }

        /// Verifica si la contraseña suministrada coincide con la almacenada.
        public bool VerificarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                return false;

            return _contrasena == contrasena;
        }

        /// Activa el usuario dentro del sistema.
        public void Activar()
        {
            if (_activo)
                return;

            _activo = true;
            ActualizarFechaModificacion();
        }

        /// Desactiva lógicamente el usuario dentro del sistema.
        public void Desactivar()
        {
            if (!_activo)
                return;

            _activo = false;
            ActualizarFechaModificacion();
        }

        /// Devuelve el rol lógico del usuario dentro del sistema.
        public virtual string ObtenerRol()
        {
            return "Usuario";
        }

        /// Devuelve una representación legible del perfil del usuario.
        public virtual string MostrarPerfil()
        {
            return $"ID: {Id} | Nombre: {Nombre} | Correo: {Correo} | Rol: {ObtenerRol()} | Activo: {Activo}";
        }

        #endregion

        #region Métodos protegidos

        /// Actualiza la fecha de modificación de la entidad.
        protected void ActualizarFechaModificacion()
        {
            _fechaActualizacion = DateTime.UtcNow;
        }

        #endregion

        #region Métodos privados de validación

        /// Valida que el identificador sea mayor que cero.
        private static int ValidarId(int id)
        {
            if (id <= 0)
                throw new UsuarioNoValidoException("El Id del usuario debe ser mayor que cero.");

            return id;
        }

        /// Valida el nombre del usuario según las reglas mínimas del dominio.
        private static string ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new UsuarioNoValidoException("El nombre del usuario es obligatorio.");

            string nombreNormalizado = nombre.Trim();

            if (nombreNormalizado.Length < LongitudMinimaNombre)
                throw new UsuarioNoValidoException($"El nombre del usuario debe tener al menos {LongitudMinimaNombre} caracteres.");

            if (nombreNormalizado.Length > LongitudMaximaNombre)
                throw new UsuarioNoValidoException($"El nombre del usuario no puede superar los {LongitudMaximaNombre} caracteres.");

            return nombreNormalizado;
        }

        /// Valida el formato del correo electrónico y lo normaliza.
        private static string ValidarCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new UsuarioNoValidoException("El correo electrónico es obligatorio.");

            string correoNormalizado = correo.Trim().ToLowerInvariant();

            if (!Regex.IsMatch(correoNormalizado, PatronCorreo))
                throw new UsuarioNoValidoException("El formato del correo electrónico no es válido.");

            return correoNormalizado;
        }

        /// Valida la contraseña según reglas mínimas del dominio.
        private static string ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                throw new UsuarioNoValidoException("La contraseña es obligatoria.");

            if (contrasena.Length < LongitudMinimaContrasena)
                throw new UsuarioNoValidoException($"La contraseña debe tener al menos {LongitudMinimaContrasena} caracteres.");

            return contrasena;
        }

        #endregion

        #region Representación textual

        /// Devuelve una representación corta del usuario.
        public override string ToString()
        {
            return $"{Nombre} ({Correo}) - {ObtenerRol()}";
        }

        #endregion
    }
}