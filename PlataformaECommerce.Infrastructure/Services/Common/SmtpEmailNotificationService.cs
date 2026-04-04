using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Notifications;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Infrastructure.Configurations;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Implementa la entrega de notificaciones por correo utilizando SMTP.
/// </summary>
public sealed class SmtpEmailNotificationService : IEmailNotificationService
{
    private readonly SmtpEmailSettings _settings;
    private readonly ILogger<SmtpEmailNotificationService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SmtpEmailNotificationService"/>.
    /// </summary>
    public SmtpEmailNotificationService(
        IOptions<SmtpEmailSettings> settings,
        ILogger<SmtpEmailNotificationService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<Result> SendAccountEmailConfirmationAsync(
        AccountEmailConfirmationNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string subject = "Confirma tu cuenta";
        string body = $"""
Hola {notification.RecipientName},

Gracias por crear tu cuenta en Plataforma ECommerce.

Confirma tu correo electrónico usando este enlace seguro:
{notification.ConfirmationUrl}

Si no reconoces este registro, puedes ignorar este mensaje.

Plataforma ECommerce
""";

        return SendEmailAsync(notification.ToEmail, notification.RecipientName, subject, body, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> SendPasswordResetEmailAsync(
        PasswordResetEmailNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string subject = "Recuperación de contraseña";
        string body = $"""
Hola {notification.RecipientName},

Recibimos una solicitud para restablecer la contraseña de tu cuenta.

Usa este enlace seguro para continuar:
{notification.ResetUrl}

El enlace estará disponible hasta {notification.ExpiresAtUtc:dd/MM/yyyy HH:mm} UTC.

Si no solicitaste este cambio, puedes ignorar este mensaje.

Plataforma ECommerce
""";

        return SendEmailAsync(notification.ToEmail, notification.RecipientName, subject, body, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> SendOrderConfirmationEmailAsync(
        OrderConfirmationEmailNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string subject = $"Confirmación de compra {notification.OrderId}";
        StringBuilder builder = new();
        builder.AppendLine($"Hola {notification.RecipientName},");
        builder.AppendLine();
        builder.AppendLine($"Tu pedido {notification.OrderId} fue confirmado correctamente.");
        builder.AppendLine($"Total: {notification.Currency} {notification.TotalAmount:N2}");
        builder.AppendLine($"Método de pago: {ResolvePaymentMethodLabel(notification.PaymentMethod)}");

        if (!string.IsNullOrWhiteSpace(notification.ShippingAddressSummary))
        {
            builder.AppendLine($"Dirección de envío: {notification.ShippingAddressSummary}");
        }

        if (notification.Items.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Productos:");

            foreach (OrderConfirmationEmailItem item in notification.Items)
            {
                builder.AppendLine($"- {item.ProductName} ({item.ProductSku}) x {item.Quantity}: {item.Currency} {item.Subtotal:N2}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Gracias por comprar en Plataforma ECommerce.");

        return SendEmailAsync(notification.ToEmail, notification.RecipientName, subject, builder.ToString(), cancellationToken);
    }

    private async Task<Result> SendEmailAsync(
        string toEmail,
        string recipientName,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "La entrega SMTP está deshabilitada. Se omitió el correo a {ToEmail} con asunto {Subject}.",
                toEmail,
                subject);
            return Result.Success();
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return Result.Failure(Error.Validation("Notifications.InvalidRecipient", "El destinatario del correo es obligatorio."));
        }

        try
        {
            using MailMessage message = new(
                new MailAddress(_settings.FromAddress, _settings.FromDisplayName),
                new MailAddress(toEmail, recipientName))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            using SmtpClient client = BuildClient();
            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message, cancellationToken);
            return Result.Success();
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "No fue posible construir el correo para {ToEmail}.", toEmail);
            return Result.Failure(Error.Validation("Notifications.InvalidEmailAddress", "La dirección de correo configurada no es válida."));
        }
        catch (FormatException exception)
        {
            _logger.LogWarning(exception, "No fue posible interpretar una dirección de correo para {ToEmail}.", toEmail);
            return Result.Failure(Error.Validation("Notifications.InvalidEmailAddress", "La dirección de correo configurada no es válida."));
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "No fue posible preparar el envío SMTP hacia {ToEmail}.", toEmail);
            return Result.Failure(Error.Failure("Notifications.EmailDeliveryFailed", "No fue posible entregar el correo electrónico solicitado."));
        }
        catch (SmtpException exception)
        {
            _logger.LogError(exception, "El servidor SMTP rechazó el correo hacia {ToEmail}.", toEmail);
            return Result.Failure(Error.Failure("Notifications.EmailDeliveryFailed", "No fue posible entregar el correo electrónico solicitado."));
        }
    }

    private SmtpClient BuildClient()
    {
        SmtpClient client = new(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
        {
            client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
        }

        return client;
    }

    private static string ResolvePaymentMethodLabel(MetodoPagoPedido? paymentMethod)
    {
        return paymentMethod switch
        {
            MetodoPagoPedido.Tarjeta => "Tarjeta",
            MetodoPagoPedido.Pse => "PSE",
            MetodoPagoPedido.TransferenciaBancaria => "Transferencia bancaria",
            MetodoPagoPedido.ContraEntrega => "Contra entrega",
            _ => "No definido"
        };
    }
}
