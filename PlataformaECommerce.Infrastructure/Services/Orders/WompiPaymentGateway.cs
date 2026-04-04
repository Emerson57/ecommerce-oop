using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Infrastructure.Configurations;

namespace PlataformaECommerce.Infrastructure.Services.Orders;

/// <summary>
/// Implementa la integración con el checkout hospedado de Wompi.
/// </summary>
public sealed class WompiPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly WompiPaymentGatewaySettings _settings;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="WompiPaymentGateway"/>.
    /// </summary>
    public WompiPaymentGateway(HttpClient httpClient, IOptions<WompiPaymentGatewaySettings> settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    public Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
        PaymentGatewayCheckoutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_settings.Enabled)
        {
            return Task.FromResult(Result.Failure<PaymentCheckoutSessionDto>(
                Error.Conflict("Payments.GatewayDisabled", "La pasarela de pagos externa no está configurada en este entorno.")));
        }

        if (string.IsNullOrWhiteSpace(request.ReturnUrl))
        {
            return Task.FromResult(Result.Failure<PaymentCheckoutSessionDto>(
                Error.Validation("Payments.ReturnUrlRequired", "La URL de retorno del pago es obligatoria.")));
        }

        long amountInCents = Convert.ToInt64(decimal.Round(request.Amount * 100m, 0, MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture);
        string currency = request.Currency.Trim().ToUpperInvariant();
        string integritySignature = BuildIntegritySignature(request.PaymentReference, amountInCents, currency);
        string checkoutUrl = BuildCheckoutUrl(request, amountInCents, currency, integritySignature);

        return Task.FromResult(Result.Success(new PaymentCheckoutSessionDto
        {
            Provider = _settings.ProviderName,
            CheckoutUrl = checkoutUrl,
            PaymentReference = request.PaymentReference,
            OrderId = request.OrderId
        }));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentGatewayTransactionDto>> VerifyTransactionAsync(
        string gatewayTransactionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gatewayTransactionId))
        {
            return Result.Failure<PaymentGatewayTransactionDto>(
                Error.Validation("Payments.TransactionIdRequired", "El identificador de la transacción externa es obligatorio."));
        }

        if (!_settings.Enabled)
        {
            return Result.Failure<PaymentGatewayTransactionDto>(
                Error.Conflict("Payments.GatewayDisabled", "La pasarela de pagos externa no está configurada en este entorno."));
        }

        using HttpResponseMessage response = await _httpClient.GetAsync(gatewayTransactionId.Trim(), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<PaymentGatewayTransactionDto>(
                Error.Failure("Payments.ProviderUnavailable", "No fue posible validar la transacción contra la pasarela de pagos."));
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("data", out JsonElement dataElement))
        {
            return Result.Failure<PaymentGatewayTransactionDto>(
                Error.Failure("Payments.InvalidProviderResponse", "La respuesta de la pasarela no contiene la transacción esperada."));
        }

        string status = ReadString(dataElement, "status");
        long amountInCents = ReadInt64(dataElement, "amount_in_cents");

        return Result.Success(new PaymentGatewayTransactionDto
        {
            Provider = _settings.ProviderName,
            GatewayTransactionId = ReadString(dataElement, "id"),
            PaymentReference = ReadString(dataElement, "reference"),
            Status = MapStatus(status),
            PaymentMethod = ReadString(dataElement, "payment_method_type"),
            Amount = amountInCents / 100m,
            Currency = ReadString(dataElement, "currency"),
            PaidAtUtc = ReadNullableUtcDateTime(dataElement, "finalized_at")
        });
    }

    private string BuildCheckoutUrl(
        PaymentGatewayCheckoutRequestDto request,
        long amountInCents,
        string currency,
        string integritySignature)
    {
        Dictionary<string, string> parameters = new(StringComparer.Ordinal)
        {
            ["public-key"] = _settings.PublicKey,
            ["currency"] = currency,
            ["amount-in-cents"] = amountInCents.ToString(CultureInfo.InvariantCulture),
            ["reference"] = request.PaymentReference,
            ["redirect-url"] = request.ReturnUrl.Trim(),
            ["signature:integrity"] = integritySignature
        };

        string queryString = string.Join("&", parameters.Select(parameter =>
            $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));

        return $"{_settings.CheckoutBaseUrl.TrimEnd('/')}?{queryString}";
    }

    private string BuildIntegritySignature(string paymentReference, long amountInCents, string currency)
    {
        string payload = string.Concat(paymentReference, amountInCents.ToString(CultureInfo.InvariantCulture), currency, _settings.IntegritySecret);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ReadString(JsonElement dataElement, string propertyName)
    {
        return dataElement.TryGetProperty(propertyName, out JsonElement property)
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static long ReadInt64(JsonElement dataElement, string propertyName)
    {
        if (!dataElement.TryGetProperty(propertyName, out JsonElement property))
        {
            return 0;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out long numberValue))
        {
            return numberValue;
        }

        return long.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsedValue)
            ? parsedValue
            : 0;
    }

    private static DateTime? ReadNullableUtcDateTime(JsonElement dataElement, string propertyName)
    {
        if (!dataElement.TryGetProperty(propertyName, out JsonElement property))
        {
            return null;
        }

        string? rawValue = property.GetString();
        if (!DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedValue))
        {
            return null;
        }

        return parsedValue.UtcDateTime;
    }

    private static PaymentGatewayTransactionStatus MapStatus(string providerStatus)
    {
        return providerStatus.Trim().ToUpperInvariant() switch
        {
            "APPROVED" => PaymentGatewayTransactionStatus.Approved,
            "PENDING" => PaymentGatewayTransactionStatus.Pending,
            "DECLINED" or "VOIDED" or "ERROR" => PaymentGatewayTransactionStatus.Declined,
            _ => PaymentGatewayTransactionStatus.Error
        };
    }
}
