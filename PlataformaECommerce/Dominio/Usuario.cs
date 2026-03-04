using System;
using System.Text.RegularExpressions;

namespace PlataformaECommerce.Dominio
{
    public abstract class Usuario
    {
        #region Campos privados (estado interno)

        private int _id;
        private string _nombre;
        private string _correo;
        private string _contrasena;

        #endregion

        #region Propiedades públicas

        /// Identificador único del usuario.
        public int Id
        {
            get => _id;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El Id del usuario debe ser mayor que cero.", nameof(Id));

                _id = value;
            }
        }

        /// Nombre completo del usuario.
        public string Nombre
        {
            get => _nombre;
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío.", nameof(Nombre));

                if (value.Length > 100)
                    throw new ArgumentException("El nombre no puede superar los 100 caracteres.", nameof(Nombre));

                _nombre = value.Trim();
            }
        }

        /// Correo electrónico del usuario.
        public string Correo
        {
            get => _correo;
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El correo no puede estar vacío.", nameof(Correo));

                if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new ArgumentException("El formato del correo no es válido.", nameof(Correo));

                _correo = value.Trim().ToLower();
            }
        }

        /// Contraseña del usuario.
        protected string Contrasena
        {
            get => _contrasena;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La contraseña no puede estar vacía.", nameof(Contrasena));

                if (value.Length < 6)
                    throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.", nameof(Contrasena));

                _contrasena = value;
            }
        }

        #endregion

        #region Constructor

        /// Constructor base para inicializar los datos fundamentales del usuario.
        protected Usuario(int id, string nombre, string correo, string contrasena)
        {
            Id = id;
            Nombre = nombre;
            Correo = correo;
            Contrasena = contrasena;
        }

        #endregion

        #region Comportamiento común

        /// Permite actualizar los datos básicos del usuario.
        public virtual void ActualizarDatos(string nombre, string correo)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
                Nombre = nombre;

            if (!string.IsNullOrWhiteSpace(correo))
                Correo = correo;
        }

        /// Devuelve el rol del usuario dentro del sistema.
        public virtual string ObtenerRol()
        {
            return "Usuario";
        }

        /// Devuelve una representación legible del perfil del usuario.
        public virtual string MostrarPerfil()
        {
            return $"ID: {Id} | Nombre: {Nombre} | Correo: {Correo} | Rol: {ObtenerRol()}";
        }

        #endregion

        #region Métodos utilitarios

        /// Representación corta del usuario para logs o debugging.
        public override string ToString()
        {
            return $"{Nombre} ({Correo})";
        }

        #endregion
    }
}