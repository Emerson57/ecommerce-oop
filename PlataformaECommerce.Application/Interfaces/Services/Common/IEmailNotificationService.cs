using PlataformaECommerce.Application.Common.Notifications;
using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Interfaces.Services.Common;

/// <summary>
/// Define la frontera para entregar notificaciones de correo electrónico del sistema.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Envía un correo de confirmación de cuenta.
    /// </summary>
    Task<Result> SendAccountEmailConfirmationAsync(
        AccountEmailConfirmationNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía un correo de recuperación de contraseña.
    /// </summary>
    Task<Result> SendPasswordResetEmailAsync(
        PasswordResetEmailNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía un correo de confirmación de compra.
    /// </summary>
    Task<Result> SendOrderConfirmationEmailAsync(
        OrderConfirmationEmailNotification notification,
        CancellationToken cancellationToken = default);
}
