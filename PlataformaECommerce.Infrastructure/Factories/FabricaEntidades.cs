using System;
using PlataformaECommerce.Domain.Entities;

namespace PlataformaECommerce.Infrastructure.Factories
{
    public static class FabricaEntidades
    {
        // ==========================================================
        // CREACIÓN DE PRODUCTOS
        // ==========================================================

        /// Crea una instancia de ProductoDigital.
        public static Producto CrearProductoDigital(
            int id,
            string nombre,
            string descripcion,
            decimal precio,
            int stock,
            string formatoArchivo,
            decimal tamanoMB)
        {
            return new ProductoDigital(
                id,
                nombre,
                descripcion,
                precio,
                stock,
                formatoArchivo,
                tamanoMB
            );
        }

        /// Crea una instancia de ProductoFisico.
        public static Producto CrearProductoFisico(
            int id,
            string nombre,
            string descripcion,
            decimal precio,
            int stock,
            decimal pesoKg,
            decimal altoCm,
            decimal anchoCm,
            decimal largoCm)
        {
            return new ProductoFisico(
                id,
                nombre,
                descripcion,
                precio,
                stock,
                pesoKg,
                altoCm,
                anchoCm,
                largoCm
            );
        }

        // ==========================================================
        // CREACIÓN DE USUARIOS
        // ==========================================================

        /// Crea una instancia de Cliente.
        public static Usuario CrearCliente(
            int id,
            string nombre,
            string correo,
            string contrasena)
        {
            return new Cliente(
                id,
                nombre,
                correo,
                contrasena
            );
        }

        /// Crea una instancia de Administrador.
        public static Usuario CrearAdministrador(
            int id,
            string nombre,
            string correo,
            string contrasena,
            string area = "Operaciones")
        {
            return new Administrador(
                id,
                nombre,
                correo,
                contrasena,
                area
            );
        }

        // ==========================================================
        // FACTORY GENÉRICO
        // ==========================================================

        /// Crea productos según el tipo especificado.
        public static Producto CrearProductoPorTipo(
        string tipoProducto,
        int id,
        string nombre,
        string descripcion,
        decimal precio,
        int stock,
        params object[] parametrosExtra)
        {
            if (string.IsNullOrWhiteSpace(tipoProducto))
                throw new ArgumentException("El tipo de producto es obligatorio.");

            if (parametrosExtra == null)
                throw new ArgumentNullException(nameof(parametrosExtra));

            tipoProducto = tipoProducto.Trim().ToLowerInvariant();

            switch (tipoProducto)
            {
                case "digital":

                    if (parametrosExtra.Length < 2)
                        throw new ArgumentException("ProductoDigital requiere formatoArchivo y tamanoMB.");

                    return new ProductoDigital(
                        id,
                        nombre,
                        descripcion,
                        precio,
                        stock,
                        (string)parametrosExtra[0],
                        (decimal)parametrosExtra[1]
                    );

                case "fisico":

                    if (parametrosExtra.Length < 4)
                        throw new ArgumentException("ProductoFisico requiere pesoKg, altoCm, anchoCm, largoCm.");

                    return new ProductoFisico(
                        id,
                        nombre,
                        descripcion,
                        precio,
                        stock,
                        (decimal)parametrosExtra[0],
                        (decimal)parametrosExtra[1],
                        (decimal)parametrosExtra[2],
                        (decimal)parametrosExtra[3]
                    );

                default:
                    throw new InvalidOperationException($"Tipo de producto no soportado: {tipoProducto}");
            }
        }
    }
}