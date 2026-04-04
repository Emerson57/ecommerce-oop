using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Infrastructure.Services.Users;

/// <summary>
/// Implementa la emisión y validación de tokens temporales de confirmación de correo.
/// </summary>
public sealed class EmailConfirmationTokenService : IEmailConfirmationTokenService
{
    private const string ProtectorPurpose = "PlataformaECommerce.Users.EmailConfirmation.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ITimeLimitedDataProtector _protector;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="EmailConfirmationTokenService"/>.
    /// </summary>
    public EmailConfirmationTokenService(IDataProtectionProvider dataProtectionProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose).ToTimeLimitedDataProtector();
    }

    /// <inheritdoc />
    public string GenerateToken(Usuario usuario, TimeSpan lifetime)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "La vigencia del token debe ser mayor que cero.");
        }

        EmailConfirmationTokenPayload payload = new()
        {
            UserId = usuario.Id,
            Email = usuario.CorreoElectronico.Value,
            UserVersionTicks = ResolveUserVersionTicks(usuario)
        };

        string rawPayload = JsonSerializer.Serialize(payload, JsonOptions);
        byte[] protectedPayload = _protector.Protect(Encoding.UTF8.GetBytes(rawPayload), lifetime);
        return WebEncoders.Base64UrlEncode(protectedPayload);
    }

    /// <inheritdoc />
    public EmailConfirmationTokenValidationDto? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            byte[] protectedPayload = WebEncoders.Base64UrlDecode(token.Trim());
            byte[] rawPayload = _protector.Unprotect(protectedPayload, out DateTimeOffset expiresAtUtc);
            EmailConfirmationTokenPayload? payload = JsonSerializer.Deserialize<EmailConfirmationTokenPayload>(rawPayload, JsonOptions);

            if (payload is null || payload.UserId == Guid.Empty || string.IsNullOrWhiteSpace(payload.Email))
            {
                return null;
            }

            return new EmailConfirmationTokenValidationDto
            {
                UserId = payload.UserId,
                Email = payload.Email,
                UserVersionTicks = payload.UserVersionTicks,
                ExpiresAtUtc = expiresAtUtc.UtcDateTime
            };
        }
        catch (FormatException)
        {
            return null;
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static long ResolveUserVersionTicks(Usuario usuario)
    {
        return (usuario.FechaActualizacionUtc ?? usuario.FechaCreacionUtc).Ticks;
    }

    private sealed record EmailConfirmationTokenPayload
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public long UserVersionTicks { get; init; }
    }
}
