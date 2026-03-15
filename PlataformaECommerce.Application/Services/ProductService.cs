using PlataformaECommerce.Application.DTOs.Products;
using PlataformaECommerce.Application.Interfaces.Repositories;
using PlataformaECommerce.Application.Interfaces.Services;
using PlataformaECommerce.Domain.Entities;

namespace PlataformaECommerce.Application.Services
{
    public sealed class ProductService : IProductoService
    {
        #region Campos privados

        private readonly IProductoRepository _productoRepository;
        private readonly IProductoAuditRepository _productoAuditRepository;
        private readonly IUnitOfWork? _unitOfWork;

        #endregion

        #region Constructor

        /// Crea una nueva instancia del servicio de productos.
        public ProductService(
            IProductoRepository productoRepository,
            IProductoAuditRepository productoAuditRepository,
            IUnitOfWork? unitOfWork = null)
        {
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _productoAuditRepository = productoAuditRepository ?? throw new ArgumentNullException(nameof(productoAuditRepository));
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region Métodos públicos

        /// Obtiene todos los productos registrados en el sistema.
        public async Task<IReadOnlyList<ProductResponse>> ObtenerTodosAsync()
        {
            IReadOnlyList<Producto> productos = await _productoRepository.ObtenerTodosAsync();

            return productos
                .Select(MapearAResponse)
                .ToList()
                .AsReadOnly();
        }

        /// Obtiene un producto por su identificador.
        public async Task<ProductResponse?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El Id del producto debe ser mayor que cero.");

            Producto? producto = await _productoRepository.ObtenerPorIdAsync(id);

            return producto is null ? null : MapearAResponse(producto);
        }

        /// Crea un nuevo producto físico o digital según el tipo indicado en la solicitud.
        public async Task<ProductResponse> CrearAsync(CreateProductRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request), "La solicitud de creación no puede ser nula.");

            ValidarRequestCreacion(request);

            Producto producto = CrearEntidadDesdeRequest(request);

            await _productoRepository.AgregarAsync(producto);

            if (_unitOfWork is not null)
                await _unitOfWork.GuardarCambiosAsync();

            await _productoAuditRepository.RegistrarEventoAsync(
                producto.Id,
                "CREACION",
                $"Se creó el producto '{producto.Nombre}' de tipo '{ObtenerTipoProducto(producto)}'.",
                "Sistema");

            return MapearAResponse(producto);
        }

        /// Actualiza un producto existente.
        public async Task<ProductResponse?> ActualizarAsync(int id, UpdateProductRequest request)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El Id del producto debe ser mayor que cero.");

            if (request is null)
                throw new ArgumentNullException(nameof(request), "La solicitud de actualización no puede ser nula.");

            ValidarRequestActualizacion(request);

            Producto? producto = await _productoRepository.ObtenerPorIdAsync(id);

            if (producto is null)
                return null;

            ActualizarEntidad(producto, request);

            await _productoRepository.ActualizarAsync(producto);

            if (_unitOfWork is not null)
                await _unitOfWork.GuardarCambiosAsync();

            await _productoAuditRepository.RegistrarEventoAsync(
                producto.Id,
                "ACTUALIZACION",
                $"Se actualizó el producto '{producto.Nombre}'.",
                "Sistema");

            return MapearAResponse(producto);
        }

        /// Elimina un producto del sistema por su identificador.
        public async Task<bool> EliminarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El Id del producto debe ser mayor que cero.");

            Producto? producto = await _productoRepository.ObtenerPorIdAsync(id);

            if (producto is null)
                return false;

            await _productoRepository.EliminarAsync(id);

            if (_unitOfWork is not null)
                await _unitOfWork.GuardarCambiosAsync();

            await _productoAuditRepository.RegistrarEventoAsync(
                id,
                "ELIMINACION",
                $"Se eliminó el producto '{producto.Nombre}'.",
                "Sistema");

            return true;
        }

        #endregion

        #region Métodos privados de creación y actualización

        /// Crea una entidad de dominio a partir de la solicitud de creación.
        private static Producto CrearEntidadDesdeRequest(CreateProductRequest request)
        {
            int idTemporal = GenerarIdTemporal();

            if (EsTipoFisico(request.TipoProducto))
            {
                return new ProductoFisico(
                    idTemporal,
                    request.Nombre,
                    request.Descripcion,
                    request.Precio,
                    request.Stock,
                    request.PesoKg!.Value,
                    request.AltoCm!.Value,
                    request.AnchoCm!.Value,
                    request.LargoCm!.Value);
            }

            if (EsTipoDigital(request.TipoProducto))
            {
                return new ProductoDigital(
                    idTemporal,
                    request.Nombre,
                    request.Descripcion,
                    request.Precio,
                    request.Stock,
                    request.FormatoArchivo!,
                    request.TamanoMB!.Value);
            }

            throw new InvalidOperationException("El tipo de producto especificado no es válido.");
        }

        /// Aplica los cambios del DTO de actualización sobre una entidad existente.
        private static void ActualizarEntidad(Producto producto, UpdateProductRequest request)
        {
            producto.ActualizarInformacionBasica(request.Nombre, request.Descripcion);
            producto.ActualizarPrecio(request.Precio);
            producto.ActualizarStock(request.Stock);

            if (producto is ProductoFisico productoFisico)
            {
                if (!EsTipoFisico(request.TipoProducto))
                    throw new InvalidOperationException("No se puede actualizar un producto físico usando un tipo distinto.");

                productoFisico.ActualizarInformacionFisica(
                    request.PesoKg!.Value,
                    request.AltoCm!.Value,
                    request.AnchoCm!.Value,
                    request.LargoCm!.Value);
            }
            else if (producto is ProductoDigital productoDigital)
            {
                if (!EsTipoDigital(request.TipoProducto))
                    throw new InvalidOperationException("No se puede actualizar un producto digital usando un tipo distinto.");

                productoDigital.ActualizarInformacionDigital(
                    request.FormatoArchivo!,
                    request.TamanoMB!.Value);
            }
        }

        #endregion

        #region Métodos privados de validación

        /// Valida los datos de entrada para la creación de productos.
        private static void ValidarRequestCreacion(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TipoProducto))
                throw new ArgumentException("El tipo de producto es obligatorio.", nameof(request.TipoProducto));

            ValidarCamposSegunTipo(request.TipoProducto,
                request.FormatoArchivo,
                request.TamanoMB,
                request.PesoKg,
                request.AltoCm,
                request.AnchoCm,
                request.LargoCm);
        }

        /// Valida los datos de entrada para la actualización de productos.
        private static void ValidarRequestActualizacion(UpdateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TipoProducto))
                throw new ArgumentException("El tipo de producto es obligatorio.", nameof(request.TipoProducto));

            ValidarCamposSegunTipo(request.TipoProducto,
                request.FormatoArchivo,
                request.TamanoMB,
                request.PesoKg,
                request.AltoCm,
                request.AnchoCm,
                request.LargoCm);
        }

        /// Valida los campos específicos de acuerdo con el tipo de producto.
        private static void ValidarCamposSegunTipo(
            string tipoProducto,
            string? formatoArchivo,
            decimal? tamanoMB,
            decimal? pesoKg,
            decimal? altoCm,
            decimal? anchoCm,
            decimal? largoCm)
        {
            if (EsTipoDigital(tipoProducto))
            {
                if (string.IsNullOrWhiteSpace(formatoArchivo))
                    throw new ArgumentException("El formato de archivo es obligatorio para productos digitales.");

                if (!tamanoMB.HasValue || tamanoMB.Value <= 0)
                    throw new ArgumentException("El tamaño en MB es obligatorio y debe ser mayor que cero para productos digitales.");

                return;
            }

            if (EsTipoFisico(tipoProducto))
            {
                if (!pesoKg.HasValue || pesoKg.Value <= 0)
                    throw new ArgumentException("El peso es obligatorio y debe ser mayor que cero para productos físicos.");

                if (!altoCm.HasValue || altoCm.Value <= 0)
                    throw new ArgumentException("El alto es obligatorio y debe ser mayor que cero para productos físicos.");

                if (!anchoCm.HasValue || anchoCm.Value <= 0)
                    throw new ArgumentException("El ancho es obligatorio y debe ser mayor que cero para productos físicos.");

                if (!largoCm.HasValue || largoCm.Value <= 0)
                    throw new ArgumentException("El largo es obligatorio y debe ser mayor que cero para productos físicos.");

                return;
            }

            throw new ArgumentException("El tipo de producto debe ser 'Fisico' o 'Digital'.", nameof(tipoProducto));
        }

        #endregion

        #region Métodos privados auxiliares

        /// Convierte una entidad de dominio Producto en un DTO de respuesta.
        private static ProductResponse MapearAResponse(Producto producto)
        {
            ProductResponse response = new()
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Activo = producto.Activo,
                TipoProducto = ObtenerTipoProducto(producto),
                FechaCreacion = producto.FechaCreacion,
                FechaActualizacion = producto.FechaActualizacion
            };

            if (producto is ProductoDigital digital)
            {
                response.FormatoArchivo = digital.FormatoArchivo;
                response.TamanoMB = digital.TamanoMB;
            }
            else if (producto is ProductoFisico fisico)
            {
                response.PesoKg = fisico.PesoKg;
                response.AltoCm = fisico.AltoCm;
                response.AnchoCm = fisico.AnchoCm;
                response.LargoCm = fisico.LargoCm;
                response.VolumenCm3 = fisico.VolumenCm3;
            }

            return response;
        }

        /// Devuelve el tipo lógico del producto a partir de su tipo real.
        private static string ObtenerTipoProducto(Producto producto)
        {
            return producto switch
            {
                ProductoFisico => "Fisico",
                ProductoDigital => "Digital",
                _ => "Desconocido"
            };
        }

        /// Determina si el tipo corresponde a un producto físico.
        private static bool EsTipoFisico(string tipoProducto)
        {
            return tipoProducto.Trim().Equals("Fisico", StringComparison.OrdinalIgnoreCase);
        }

        /// Determina si el tipo corresponde a un producto digital.
        private static bool EsTipoDigital(string tipoProducto)
        {
            return tipoProducto.Trim().Equals("Digital", StringComparison.OrdinalIgnoreCase);
        }

        /// Genera un Id temporal.
        private static int GenerarIdTemporal()
        {
            return Math.Abs(Guid.NewGuid().GetHashCode());
        }

        #endregion
    }
}