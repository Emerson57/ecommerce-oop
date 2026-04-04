using System.Globalization;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.Commands;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Features.Orders.Mappings;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Application.Features.Orders.Services;

/// <summary>
/// Orquesta el registro de pagos del módulo de pedidos.
/// </summary>
public sealed class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _auditTrailService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PaymentService"/>.
    /// </summary>
    public PaymentService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IAuditTrailService auditTrailService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
    }

    /// <inheritdoc />
    public async Task<Result<OrderDetailDto>> RegisterOrderPaymentAsync(
        RegisterOrderPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = OrderServiceSupport.ValidateRegisterPaymentCommand(command);
        if (validationError is not null)
        {
            return Result.Failure<OrderDetailDto>(validationError);
        }

        return await OrderServiceSupport.ExecuteAsync(async () =>
        {
            Pedido? order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order is null)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.NotFound("Orders.NotFound", $"No se encontró un pedido con identificador '{command.OrderId}'."));
            }

            if (!string.Equals(order.Total.Currency, command.Currency?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Validation(
                        "Orders.PaymentCurrencyMismatch",
                        $"La moneda del pago '{command.Currency}' no coincide con la moneda del pedido '{order.Total.Currency}'."));
            }

            if (command.Amount != order.Total.Amount)
            {
                return Result.Failure<OrderDetailDto>(
                    Error.Validation(
                        "Orders.PaymentAmountMismatch",
                        $"El valor pagado '{command.Amount:N2}' no coincide con el total del pedido '{order.Total.Amount:N2}'."));
            }

            IReadOnlyDictionary<Guid, int> orderedProductQuantities = OrderServiceSupport.BuildOrderedProductQuantities(order);
            List<Producto> productsToUpdate = await GetProductsForStockAdjustmentAsync(orderedProductQuantities, cancellationToken);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (Producto product in productsToUpdate)
                {
                    int orderedQuantity = orderedProductQuantities[product.Id];
                    product.DisminuirStock(orderedQuantity);
                    await _productRepository.UpdateAsync(product, cancellationToken);
                }

                order.RegistrarPago();

                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            await OrderServiceSupport.AuditOrderEventAsync(
                _auditTrailService,
                order,
                "order.payment.registered",
                $"Se registró el pago del pedido '{order.Id}'.",
                new Dictionary<string, string>
                {
                    ["paymentReference"] = command.PaymentReference.Trim(),
                    ["paymentMethod"] = command.PaymentMethod.Trim(),
                    ["amount"] = command.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = command.Currency.Trim().ToUpperInvariant(),
                    ["status"] = order.Estado.ToString(),
                    ["adjustedStockProducts"] = productsToUpdate.Count.ToString(CultureInfo.InvariantCulture)
                },
                cancellationToken);

            foreach (Producto product in productsToUpdate)
            {
                int orderedQuantity = orderedProductQuantities[product.Id];
                await _auditTrailService.RegisterAsync(
                    product.Id,
                    nameof(Producto),
                    "Products",
                    "product.stock.decreased.for-order-payment",
                    $"Se descontó inventario del producto '{product.Sku.Value}' por el pago del pedido '{order.Id}'.",
                    new Dictionary<string, string>
                    {
                        ["orderId"] = order.Id.ToString(),
                        ["quantity"] = orderedQuantity.ToString(CultureInfo.InvariantCulture),
                        ["resultingStock"] = product.Stock.ToString(CultureInfo.InvariantCulture),
                        ["sku"] = product.Sku.Value
                    },
                    cancellationToken);
            }

            return Result.Success(order.ToOrderDetailDto());
        }, "Orders.Domain");
    }

    private async Task<List<Producto>> GetProductsForStockAdjustmentAsync(
        IReadOnlyDictionary<Guid, int> orderedProductQuantities,
        CancellationToken cancellationToken)
    {
        List<Producto> products = [];

        foreach (KeyValuePair<Guid, int> orderedProduct in orderedProductQuantities)
        {
            Producto? product = await _productRepository.GetByIdAsync(orderedProduct.Key, cancellationToken);
            if (product is null)
            {
                throw new InvalidOperationException($"No se encontró el producto '{orderedProduct.Key}' requerido para registrar el pago del pedido.");
            }

            products.Add(product);
        }

        return products;
    }
}
