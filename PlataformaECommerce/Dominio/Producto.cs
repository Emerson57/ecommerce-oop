using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaECommerce.Dominio
{
    public class Producto
    {
        // 1) Atributos (campos privados)
        private int _id;
        private string _nombre;
        private string _descripcion;
        private decimal _precio;
        private int _stock;

        // 2) Propiedades (get / set)
        public int Id
        {
            get { return _id; }
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El Id debe ser mayor que 0.");
                _id = value;
            }
        }

        public string Nombre
        {
            get { return _nombre; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío.");
                _nombre = value.Trim();
            }
        }

        public string Descripcion
        {
            get { return _descripcion; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La descripción no puede estar vacía.");
                _descripcion = value.Trim();
            }
        }

        public decimal Precio
        {
            get { return _precio; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("El precio debe ser mayor que 0.");
                _precio = value;
            }
        }

        public int Stock
        {
            get { return _stock; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("El stock no puede ser negativo.");
                _stock = value;
            }
        }

        // 3) Constructor
        public Producto(int id, string nombre, string descripcion, decimal precio, int stock)
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            Stock = stock;
        }

        // 4) Métodos útiles (además de las propiedades)
        public void AumentarStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que 0.");

            Stock += cantidad;
        }

        public void DisminuirStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que 0.");

            if (cantidad > Stock)
                throw new InvalidOperationException("No hay stock suficiente para disminuir esa cantidad.");

            Stock -= cantidad;
        }

        public void ActualizarPrecio(decimal nuevoPrecio)
        {
            Precio = nuevoPrecio;
        }

        public override string ToString()
        {
            return $"Producto: {Id} - {Nombre} | Precio: {Precio:C} | Stock: {Stock}";
        }
    }
}
