using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaECommerce.Dominio
{
    public class Usuario
    {
        // 🔹 Campos privados (encapsulación)
        private int _id;
        private string _nombre;
        private string _correo;
        private string _contrasena;

        // 🔹 Propiedades públicas
        public int Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        public string Nombre
        {
            get { return _nombre; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío.");
                _nombre = value;
            }
        }

        public string Correo
        {
            get { return _correo; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El correo no puede estar vacío.");
                _correo = value;
            }
        }

        public string Contrasena
        {
            get { return _contrasena; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La contraseña no puede estar vacía.");
                _contrasena = value;
            }
        }

        // 🔹 Constructor
        public Usuario(int id, string nombre, string correo, string contrasena)
        {
            Id = id;
            Nombre = nombre;
            Correo = correo;
            Contrasena = contrasena;
        }

        // 🔹 Método para mostrar información básica
        public void MostrarInformacion()
        {
            Console.WriteLine($"ID: {Id}");
            Console.WriteLine($"Nombre: {Nombre}");
            Console.WriteLine($"Correo: {Correo}");
        }

        // 🔹 Destructor (opcional)
        ~Usuario()
        {
            // En .NET raramente se usa.
            // Se incluye solo con fines académicos.
            Console.WriteLine($"Usuario {Nombre} eliminado de memoria.");
        }
    }
}
