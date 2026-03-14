using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Domain.Exceptions;
using System;

namespace PlataformaECommerce.Infrastructure.Factories
{
    public static class FabricaEntidades
    {
        #region Constantes internas de fábrica

        /// Tipo de producto digital.
        private const string TipoProductoDigital = "digital";

        /// Tipo de producto físico.
        private const string TipoProductoFisico = "fisico";

        #endregion

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
            ValidarIdEntidad(id);

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
            ValidarIdEntidad(id);

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
            ValidarIdEntidad(id);

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
            ValidarIdEntidad(id);

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
                throw new FactoryException("El tipo de producto es obligatorio.");

            if (parametrosExtra == null)
                throw new FactoryException("Los parámetros adicionales del producto no pueden ser nulos.");

            ValidarIdEntidad(id);

            tipoProducto = tipoProducto.Trim().ToLowerInvariant();

            switch (tipoProducto)
            {
                case TipoProductoDigital:

                    if (parametrosExtra.Length < 2)
                        throw new FactoryException(
                            "ProductoDigital requiere los parámetros: formatoArchivo y tamanoMB."
                        );

                    if (parametrosExtra[0] is not string formatoArchivo)
                        throw new FactoryException("El parámetro formatoArchivo debe ser de tipo string.");

                    if (parametrosExtra[1] is not decimal tamanoMB)
                        throw new FactoryException("El parámetro tamanoMB debe ser de tipo decimal.");

                    return new ProductoDigital(
                        id,
                        nombre,
                        descripcion,
                        precio,
                        stock,
                        formatoArchivo,
                        tamanoMB
                    );

                case TipoProductoFisico:

                    if (parametrosExtra.Length < 4)
                        throw new FactoryException(
                            "ProductoFisico requiere los parámetros: pesoKg, altoCm, anchoCm, largoCm."
                        );

                    if (parametrosExtra[0] is not decimal pesoKg)
                        throw new FactoryException("El parámetro pesoKg debe ser de tipo decimal.");

                    if (parametrosExtra[1] is not decimal altoCm)
                        throw new FactoryException("El parámetro altoCm debe ser de tipo decimal.");

                    if (parametrosExtra[2] is not decimal anchoCm)
                        throw new FactoryException("El parámetro anchoCm debe ser de tipo decimal.");

                    if (parametrosExtra[3] is not decimal largoCm)
                        throw new FactoryException("El parámetro largoCm debe ser de tipo decimal.");

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

                default:
                    throw new EntidadNoSoportadaException(tipoProducto, "Producto");
            }
        }

        #region Validaciones internas

        /// Valida que el identificador de la entidad sea válido.
        private static void ValidarIdEntidad(int id)
        {
            if (id <= 0)
                throw new FactoryException("El identificador de la entidad debe ser mayor que cero.");
        }

        #endregion
    }
}