using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Features.Orders.DTOs;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Infrastructure.Configurations;
using PlataformaECommerce.Infrastructure.Services.Orders;

namespace PlataformaECommerce.Tests.Infrastructure.Orders;

[TestFixture]
public class WompiPaymentGatewayTests
{
    [Test]
    public async Task CreateCheckoutSessionAsync_RequestValido_ConstruyeUrlFirmada()
    {
        WompiPaymentGateway gateway = CreateGateway();

        var result = await gateway.CreateCheckoutSessionAsync(new PaymentGatewayCheckoutRequestDto
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            PaymentMethod = MetodoPagoPedido.Tarjeta,
            Amount = 199900m,
            Currency = "COP",
            PaymentReference = "PAY-123",
            ReturnUrl = "https://shop.example.com/payments/confirm?orderId=123"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.CheckoutUrl, Does.Contain("public-key=pub_test_key"));
        Assert.That(result.Value.CheckoutUrl, Does.Contain("reference=PAY-123"));
        Assert.That(result.Value.CheckoutUrl, Does.Contain("signature%3Aintegrity="));
    }

    [Test]
    public async Task VerifyTransactionAsync_RespuestaAprobada_MapeaDatosDeTransaccion()
    {
        const string json = """
        {
          "data": {
            "id": "tx-789",
            "status": "APPROVED",
            "reference": "PAY-123",
            "payment_method_type": "CARD",
            "amount_in_cents": 19990000,
            "currency": "COP",
            "finalized_at": "2026-04-04T10:15:30Z"
          }
        }
        """;
        WompiPaymentGateway gateway = CreateGateway(json);

        var result = await gateway.VerifyTransactionAsync("tx-789");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Status, Is.EqualTo(PaymentGatewayTransactionStatus.Approved));
        Assert.That(result.Value.Amount, Is.EqualTo(199900m));
        Assert.That(result.Value.PaymentReference, Is.EqualTo("PAY-123"));
    }

    private static WompiPaymentGateway CreateGateway(string responseJson = "{\"data\":{}}")
    {
        HttpClient httpClient = new(new FakeHttpMessageHandler(responseJson))
        {
            BaseAddress = new Uri("https://production.wompi.co/v1/transactions/")
        };

        return new WompiPaymentGateway(httpClient, Options.Create(new WompiPaymentGatewaySettings
        {
            Enabled = true,
            ProviderName = "Wompi",
            CheckoutBaseUrl = "https://checkout.wompi.co/p/",
            TransactionsApiBaseUrl = "https://production.wompi.co/v1/transactions/",
            PublicKey = "pub_test_key",
            IntegritySecret = "integrity_test_secret"
        }));
    }

    private sealed class FakeHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
