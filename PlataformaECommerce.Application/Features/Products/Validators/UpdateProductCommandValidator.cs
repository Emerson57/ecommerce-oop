using FluentValidation;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Products.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="UpdateProductCommand"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las reglas de validación de entrada necesarias
/// antes de ejecutar el caso de uso de actualización de un producto.
///
/// Su responsabilidad es proteger la capa Application frente a solicitudes
/// incompletas, inconsistentes o mal formadas, permitiendo que el servicio de aplicación
/// reciba un comando previamente saneado desde el punto de vista estructural.
///
/// Las validaciones aquí definidas no reemplazan las reglas del dominio,
/// sino que actúan como una primera barrera de entrada para:
/// - endpoints HTTP,
/// - servicios de aplicación,
/// - flujos administrativos,
/// - procesos de integración.
///
/// Este validador soporta tanto productos físicos como digitales,
/// por lo que aplica reglas condicionales según el valor de
/// <see cref="UpdateProductCommand.ProductType"/>.
/// </remarks>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    #region Constantes de validación

    /// <summary>
    /// Longitud mínima permitida para el nombre del producto.
    /// </summary>
    private const int MinNameLength = 3;

    /// <summary>
    /// Longitud máxima permitida para el nombre del producto.
    /// </summary>
    private const int MaxNameLength = 150;

    /// <summary>
    /// Longitud máxima permitida para la descripción del producto.
    /// </summary>
    private const int MaxDescriptionLength = 2000;

    /// <summary>
    /// Longitud máxima permitida para el SKU del producto.
    /// </summary>
    private const int MaxSkuLength = 50;

    /// <summary>
    /// Longitud máxima permitida para el slug del producto.
    /// </summary>
    private const int MaxSlugLength = 160;

    /// <summary>
    /// Longitud máxima permitida para una URL de imagen principal.
    /// </summary>
    private const int MaxMainImageUrlLength = 1000;

    /// <summary>
    /// Longitud exacta esperada para el código de moneda.
    /// </summary>
    private const int CurrencyLength = 3;

    /// <summary>
    /// Longitud máxima permitida para una etiqueta.
    /// </summary>
    private const int MaxTagLength = 50;

    /// <summary>
    /// Cantidad máxima de etiquetas permitidas por solicitud.
    /// </summary>
    private const int MaxTagsCount = 20;

    /// <summary>
    /// Precio mínimo permitido para el producto.
    /// </summary>
    private const decimal MinPrice = 0.01m;

    /// <summary>
    /// Peso máximo permitido para productos físicos expresado en kilogramos.
    /// </summary>
    private const decimal MaxWeightKg = 1000m;

    /// <summary>
    /// Dimensión máxima permitida para productos físicos expresada en centímetros.
    /// </summary>
    private const decimal MaxDimensionCm = 500m;

    /// <summary>
    /// Longitud máxima permitida para el formato del archivo digital.
    /// </summary>
    private const int MaxFileFormatLength = 20;

    /// <summary>
    /// Tamaño máximo permitido para archivos digitales en megabytes.
    /// </summary>
    private const decimal MaxFileSizeMb = 10240m;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador
    /// <see cref="UpdateProductCommandValidator"/>.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        ConfigureIdentityRules();
        ConfigureGeneralRules();
        ConfigureCommercialRules();
        ConfigureClassificationRules();
        ConfigurePhysicalProductRules();
        ConfigureDigitalProductRules();
        ConfigureCrossFieldRules();
    }

    #endregion

    #region Métodos privados de configuración

    /// <summary>
    /// Configura las reglas relacionadas con la identidad del producto.
    /// </summary>
    private void ConfigureIdentityRules()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
                .WithMessage("El identificador del producto es obligatorio.");
    }

    /// <summary>
    /// Configura las reglas generales de texto e identificación funcional.
    /// </summary>
    private void ConfigureGeneralRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("El nombre del producto es obligatorio.")
            .MinimumLength(MinNameLength)
                .WithMessage($"El nombre del producto debe tener al menos {MinNameLength} caracteres.")
            .MaximumLength(MaxNameLength)
                .WithMessage($"El nombre del producto no puede superar los {MaxNameLength} caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithMessage("La descripción del producto es obligatoria.")
            .MaximumLength(MaxDescriptionLength)
                .WithMessage($"La descripción del producto no puede superar los {MaxDescriptionLength} caracteres.");

        RuleFor(x => x.Sku)
            .NotEmpty()
                .WithMessage("El SKU del producto es obligatorio.")
            .MaximumLength(MaxSkuLength)
                .WithMessage($"El SKU del producto no puede superar los {MaxSkuLength} caracteres.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
                .WithMessage("El SKU del producto solo puede contener letras, números, guiones y guiones bajos.");

        RuleFor(x => x.Slug)
            .NotEmpty()
                .WithMessage("El slug del producto es obligatorio.")
            .MaximumLength(MaxSlugLength)
                .WithMessage($"El slug del producto no puede superar los {MaxSlugLength} caracteres.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("El slug del producto debe contener solo letras minúsculas, números y guiones.");

        RuleFor(x => x.MainImageUrl)
            .MaximumLength(MaxMainImageUrlLength)
                .WithMessage($"La URL de la imagen principal no puede superar los {MaxMainImageUrlLength} caracteres.")
            .Must(BeAValidUrlOrRelativePath)
                .When(x => !string.IsNullOrWhiteSpace(x.MainImageUrl))
                .WithMessage("La imagen principal debe ser una URL válida o una ruta relativa válida.");
    }

    /// <summary>
    /// Configura las reglas comerciales del producto.
    /// </summary>
    private void ConfigureCommercialRules()
    {
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(MinPrice)
                .WithMessage("El precio del producto debe ser mayor que cero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
                .WithMessage("La moneda del producto es obligatoria.")
            .Length(CurrencyLength)
                .WithMessage($"La moneda del producto debe tener exactamente {CurrencyLength} caracteres.")
            .Matches(@"^[A-Za-z]{3}$")
                .WithMessage("La moneda del producto debe corresponder a un código ISO de tres letras.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
                .WithMessage("El stock del producto no puede ser negativo.");
    }

    /// <summary>
    /// Configura las reglas de clasificación y metadatos.
    /// </summary>
    private void ConfigureClassificationRules()
    {
        RuleFor(x => x.ProductType)
            .IsInEnum()
                .WithMessage("El tipo de producto indicado no es válido.");

        RuleFor(x => x.CategoryId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("La categoría del producto no puede ser un identificador vacío.");

        RuleFor(x => x.Tags)
            .Must(tags => tags is not null)
            .WithMessage("La colección de etiquetas no puede ser nula.");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= MaxTagsCount)
            .WithMessage($"No es posible registrar más de {MaxTagsCount} etiquetas por producto.");

        RuleForEach(x => x.Tags)
            .NotEmpty()
                .WithMessage("Las etiquetas del producto no pueden estar vacías.")
            .MaximumLength(MaxTagLength)
                .WithMessage($"Cada etiqueta del producto no puede superar los {MaxTagLength} caracteres.");
    }

    /// <summary>
    /// Configura las reglas específicas para productos físicos.
    /// </summary>
    private void ConfigurePhysicalProductRules()
    {
        When(x => x.ProductType == TipoProducto.Fisico, () =>
        {
            RuleFor(x => x.WeightKg)
                .NotNull()
                    .WithMessage("El peso del producto físico es obligatorio.")
                .GreaterThan(0)
                    .WithMessage("El peso del producto físico debe ser mayor que cero.")
                .LessThanOrEqualTo(MaxWeightKg)
                    .WithMessage($"El peso del producto físico no puede superar los {MaxWeightKg} kg.");

            RuleFor(x => x.HeightCm)
                .NotNull()
                    .WithMessage("El alto del producto físico es obligatorio.")
                .GreaterThan(0)
                    .WithMessage("El alto del producto físico debe ser mayor que cero.")
                .LessThanOrEqualTo(MaxDimensionCm)
                    .WithMessage($"El alto del producto físico no puede superar los {MaxDimensionCm} cm.");

            RuleFor(x => x.WidthCm)
                .NotNull()
                    .WithMessage("El ancho del producto físico es obligatorio.")
                .GreaterThan(0)
                    .WithMessage("El ancho del producto físico debe ser mayor que cero.")
                .LessThanOrEqualTo(MaxDimensionCm)
                    .WithMessage($"El ancho del producto físico no puede superar los {MaxDimensionCm} cm.");

            RuleFor(x => x.LengthCm)
                .NotNull()
                    .WithMessage("El largo del producto físico es obligatorio.")
                .GreaterThan(0)
                    .WithMessage("El largo del producto físico debe ser mayor que cero.")
                .LessThanOrEqualTo(MaxDimensionCm)
                    .WithMessage($"El largo del producto físico no puede superar los {MaxDimensionCm} cm.");

            RuleFor(x => x.RequiresShipping)
                .NotNull()
                    .WithMessage("Debe indicarse si el producto físico requiere envío.");
        });
    }

    /// <summary>
    /// Configura las reglas específicas para productos digitales.
    /// </summary>
    private void ConfigureDigitalProductRules()
    {
        When(x => x.ProductType == TipoProducto.Digital, () =>
        {
            RuleFor(x => x.FileFormat)
                .NotEmpty()
                    .WithMessage("El formato del archivo digital es obligatorio.")
                .MaximumLength(MaxFileFormatLength)
                    .WithMessage($"El formato del archivo digital no puede superar los {MaxFileFormatLength} caracteres.")
                .Matches(@"^[A-Za-z0-9]+$")
                    .WithMessage("El formato del archivo digital solo puede contener letras y números.");

            RuleFor(x => x.FileSizeMb)
                .GreaterThan(0)
                    .When(x => x.FileSizeMb.HasValue)
                    .WithMessage("El tamaño del archivo digital debe ser mayor que cero cuando se informe.")
                .LessThanOrEqualTo(MaxFileSizeMb)
                    .When(x => x.FileSizeMb.HasValue)
                    .WithMessage($"El tamaño del archivo digital no puede superar los {MaxFileSizeMb} MB.");

            RuleFor(x => x.RequiresLicense)
                .NotNull()
                    .WithMessage("Debe indicarse si el producto digital requiere licencia.");
        });
    }

    /// <summary>
    /// Configura reglas cruzadas de consistencia entre campos.
    /// </summary>
    private void ConfigureCrossFieldRules()
    {
        When(x => x.ProductType == TipoProducto.Fisico, () =>
        {
            RuleFor(x => x.FileFormat)
                .Must(string.IsNullOrWhiteSpace)
                .WithMessage("Un producto físico no debe incluir formato de archivo digital.");

            RuleFor(x => x.FileSizeMb)
                .Must(value => value is null)
                .WithMessage("Un producto físico no debe incluir tamaño de archivo digital.");

            RuleFor(x => x.RequiresLicense)
                .Must(value => value is null)
                .WithMessage("Un producto físico no debe incluir configuración de licencia digital.");
        });

        When(x => x.ProductType == TipoProducto.Digital, () =>
        {
            RuleFor(x => x.WeightKg)
                .Must(value => value is null)
                .WithMessage("Un producto digital no debe incluir peso físico.");

            RuleFor(x => x.HeightCm)
                .Must(value => value is null)
                .WithMessage("Un producto digital no debe incluir alto físico.");

            RuleFor(x => x.WidthCm)
                .Must(value => value is null)
                .WithMessage("Un producto digital no debe incluir ancho físico.");

            RuleFor(x => x.LengthCm)
                .Must(value => value is null)
                .WithMessage("Un producto digital no debe incluir largo físico.");

            RuleFor(x => x.RequiresShipping)
                .Must(value => value is null)
                .WithMessage("Un producto digital no debe incluir configuración de envío físico.");
        });
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Determina si el valor suministrado corresponde a una URL válida
    /// o a una ruta relativa razonable para una imagen.
    /// </summary>
    /// <param name="value">Valor a validar.</param>
    /// <returns>
    /// <see langword="true"/> si el valor es aceptable;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool BeAValidUrlOrRelativePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        string normalizedValue = value.Trim();

        if (Uri.TryCreate(normalizedValue, UriKind.Absolute, out Uri? absoluteUri))
        {
            return absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps;
        }

        return normalizedValue.StartsWith("/", StringComparison.Ordinal)
               || normalizedValue.StartsWith("~/", StringComparison.Ordinal)
               || normalizedValue.StartsWith("images/", StringComparison.OrdinalIgnoreCase)
               || normalizedValue.StartsWith("img/", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}