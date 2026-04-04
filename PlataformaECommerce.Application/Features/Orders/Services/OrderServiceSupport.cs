using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Domain.Entities.Orders;

namespace PlataformaECommerce.Application.Features.Orders.Services;

internal static class OrderServiceSupport
{
    internal static Task<Error?> ValidateAsync<TCommand>(
        TCommand command,
        IValidator<TCommand> validator,
        CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            validator,
            "Orders.Validation",
            "La solicitud del pedido contiene errores de validación.",
            cancellationToken);
    }

    internal static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }

    internal static Error? ValidateRegisterPaymentCommand(RegisterOrderPaymentCommand command)
    {
        if (command.OrderId == Guid.Empty)
        {
            return Error.Validation("Orders.InvalidId", "El identificador del pedido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(command.PaymentReference))
        {
            return Error.Validation("Orders.InvalidPaymentReference", "La referencia del pago es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(command.PaymentMethod))
        {
            return Error.Validation("Orders.InvalidPaymentMethod", "El método de pago es obligatorio.");
        }

        if (command.Amount <= 0)
        {
            return Error.Validation("Orders.InvalidPaymentAmount", "El monto del pago debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(command.Currency))
        {
            return Error.Validation("Orders.InvalidPaymentCurrency", "La moneda del pago es obligatoria.");
        }

        return null;
    }

    internal static IEnumerable<Pedido> ApplyOrdersFilter(
        IEnumerable<Pedido> orders,
        GetOrdersByCustomerIdQuery query)
    {
        IEnumerable<Pedido> filteredOrders = orders;

        if (query.CreatedFromUtc.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.FechaCreacionUtc >= query.CreatedFromUtc.Value);
        }

        if (query.CreatedToUtc.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.FechaCreacionUtc <= query.CreatedToUtc.Value);
        }

        if (query.MinTotalAmount.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.Total.Amount >= query.MinTotalAmount.Value);
        }

        if (query.MaxTotalAmount.HasValue)
        {
            filteredOrders = filteredOrders.Where(order => order.Total.Amount <= query.MaxTotalAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            filteredOrders = filteredOrders.Where(order =>
                string.Equals(order.Total.Currency, query.Currency.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (query.OnlyFinalized == true)
        {
            filteredOrders = filteredOrders.Where(order => order.EstaFinalizado());
        }

        if (query.OnlyActive == true)
        {
            filteredOrders = filteredOrders.Where(order => !order.EstaFinalizado());
        }

        return filteredOrders;
    }

    internal static IEnumerable<Pedido> ApplyOrdersSorting(
        IEnumerable<Pedido> orders,
        GetOrdersByCustomerIdQuery query)
    {
        string sortBy = query.SortBy?.Trim().ToLowerInvariant() ?? "createdat";

        return sortBy switch
        {
            "totalamount" or "total" => query.SortDescending
                ? orders.OrderByDescending(order => order.Total.Amount).ThenByDescending(order => order.FechaCreacionUtc)
                : orders.OrderBy(order => order.Total.Amount).ThenBy(order => order.FechaCreacionUtc),
            "status" => query.SortDescending
                ? orders.OrderByDescending(order => order.Estado).ThenByDescending(order => order.FechaCreacionUtc)
                : orders.OrderBy(order => order.Estado).ThenBy(order => order.FechaCreacionUtc),
            "updatedat" => query.SortDescending
                ? orders.OrderByDescending(order => order.FechaActualizacionUtc ?? order.FechaCreacionUtc)
                : orders.OrderBy(order => order.FechaActualizacionUtc ?? order.FechaCreacionUtc),
            _ => query.SortDescending
                ? orders.OrderByDescending(order => order.FechaCreacionUtc)
                : orders.OrderBy(order => order.FechaCreacionUtc)
        };
    }

    internal static Task AuditOrderEventAsync(
        IAuditTrailService auditTrailService,
        Pedido order,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(auditTrailService);
        ArgumentNullException.ThrowIfNull(order);

        return auditTrailService.RegisterAsync(
            order.Id,
            nameof(Pedido),
            "Orders",
            action,
            detail,
            metadata,
            cancellationToken);
    }

    internal static IReadOnlyDictionary<Guid, int> BuildOrderedProductQuantities(Pedido order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return order.Detalles
            .GroupBy(detail => detail.ProductoId)
            .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Cantidad));
    }
}
