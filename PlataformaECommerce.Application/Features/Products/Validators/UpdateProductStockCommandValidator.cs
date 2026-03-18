using FluentValidation;
using PlataformaECommerce.Application.Features.Products.Commands;

namespace PlataformaECommerce.Application.Features.Products.Validators;

/// <summary>
/// Validador de aplicación para el comando <see cref="UpdateProductStockCommand"/>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las reglas de validación de entrada necesarias
/// antes de ejecutar el caso de uso de actualización de inventario de un producto.
///
/// Su responsabilidad es proteger la capa Application frente a solicitudes
/// incompletas, inconsistentes o mal formadas, permitiendo que el handler
/// reciba un comando previamente saneado desde el punto de vista estructural.
///
/// Las validaciones aquí definidas no reemplazan las reglas del dominio,
/// sino que actúan como una primera barrera de entrada para:
/// - endpoints HTTP,
/// - servicios de aplicación,
/// - flujos administrativos,
/// - procesos de integración,
/// - operaciones de inventario.
///
/// Este validador está orientado específicamente a ajustes de stock,
/// por lo cual valida la coherencia mínima entre:
/// - identificador del producto,
/// - tipo de ajuste,
/// - cantidad,
/// - y motivo funcional del cambio.
/// </remarks>
public sealed class UpdateProductStockCommandValidator : AbstractValidator<UpdateProductStockCommand>
{
    #region Constantes de validación

    /// <summary>
    /// Longitud mínima permitida para el motivo del ajuste.
    /// </summary>
    private const int MinReasonLength = 3;

    /// <summary>
    /// Longitud máxima permitida para el motivo del ajuste.
    /// </summary>
    private const int MaxReasonLength = 300;

    /// <summary>
    /// Longitud máxima permitida para la referencia externa.
    /// </summary>
    private const int MaxExternalReferenceLength = 100;

    /// <summary>
    /// Cantidad máxima permitida para una operación de ajuste.
    /// </summary>
    /// <remarks>
    /// Este valor actúa como barrera defensiva a nivel de Application
    /// para evitar entradas desproporcionadas o probablemente erróneas.
    /// Las reglas definitivas del negocio siguen perteneciendo al dominio.
    /// </remarks>
    private const int MaxStockOperationQuantity = 1_000_000;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia del validador
    /// <see cref="UpdateProductStockCommandValidator"/>.
    /// </summary>
    public UpdateProductStockCommandValidator()
    {
        ConfigureIdentityRules();
        ConfigureOperationRules();
        ConfigureReasonRules();
        ConfigureOptionalContextRules();
        ConfigureCrossFieldRules();
    }

    #endregion

    #region Métodos privados de configuración

    /// <summary>
    /// Configura las reglas relacionadas con la identidad del producto.
    /// </summary>
    private void ConfigureIdentityRules()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El identificador del producto es obligatorio.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con la operación de inventario.
    /// </summary>
    private void ConfigureOperationRules()
    {
        RuleFor(x => x.UpdateType)
            .IsInEnum()
            .WithMessage("El tipo de actualización de inventario no es válido.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("La cantidad del ajuste de inventario debe ser mayor que cero.")
            .LessThanOrEqualTo(MaxStockOperationQuantity)
            .WithMessage($"La cantidad del ajuste de inventario no puede superar {MaxStockOperationQuantity} unidades.");
    }

    /// <summary>
    /// Configura las reglas relacionadas con el motivo funcional del ajuste.
    /// </summary>
    private void ConfigureReasonRules()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("El motivo del ajuste de inventario es obligatorio.")
            .MinimumLength(MinReasonLength)
            .WithMessage($"El motivo del ajuste de inventario debe tener al menos {MinReasonLength} caracteres.")
            .MaximumLength(MaxReasonLength)
            .WithMessage($"El motivo del ajuste de inventario no puede superar los {MaxReasonLength} caracteres.");
    }

    /// <summary>
    /// Configura las reglas de contexto opcional del comando.
    /// </summary>
    private void ConfigureOptionalContextRules()
    {
        RuleFor(x => x.RequestedByUserId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("El identificador del usuario solicitante no puede ser un valor vacío.");

        RuleFor(x => x.ExternalReference)
            .MaximumLength(MaxExternalReferenceLength)
            .WithMessage($"La referencia externa no puede superar los {MaxExternalReferenceLength} caracteres.");
    }

    /// <summary>
    /// Configura reglas cruzadas de consistencia entre campos.
    /// </summary>
    private void ConfigureCrossFieldRules()
    {
        RuleFor(x => x)
            .Must(HasConsistentQuantityForOperation)
            .WithMessage("La cantidad indicada no es consistente con la operación de inventario solicitada.");
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Determina si la cantidad indicada es consistente con la operación solicitada.
    /// </summary>
    /// <param name="command">Comando a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si la combinación es consistente;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Actualmente todas las operaciones admiten valores positivos y la semántica
    /// de aumento, disminución o asignación absoluta se expresa a través de
    /// <see cref="StockUpdateType"/>.
    /// Este método se mantiene para dejar explícito el punto de evolución futura
    /// si se agregan nuevas reglas operativas.
    /// </remarks>
    private static bool HasConsistentQuantityForOperation(UpdateProductStockCommand command)
    {
        return command.UpdateType switch
        {
            StockUpdateType.Set => command.Quantity > 0,
            StockUpdateType.Increase => command.Quantity > 0,
            StockUpdateType.Decrease => command.Quantity > 0,
            _ => false
        };
    }

    #endregion
}