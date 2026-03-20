using Microsoft.AspNetCore.Identity;
using PlataformaECommerce.Application.Interfaces.Services.Auth;

namespace PlataformaECommerce.Infrastructure.Services.Auth;

/// <summary>
/// Implementa el contrato de hashing de contraseñas de la capa Application
/// utilizando el algoritmo y las convenciones seguras provistas por ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// Esta clase encapsula completamente el detalle técnico del hashing para que la capa
/// Application permanezca desacoplada de Identity, manteniendo una frontera limpia
/// y profesional entre la lógica de negocio y la seguridad operativa.
/// </remarks>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<PasswordHasherUser> Hasher = new();
    private static readonly PasswordHasherUser User = new();

    /// <inheritdoc />
    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new ArgumentException("La contraseña en texto plano es obligatoria.", nameof(plainPassword));
        }

        return Hasher.HashPassword(User, plainPassword);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new ArgumentException("La contraseña en texto plano es obligatoria.", nameof(plainPassword));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("El hash de contraseña es obligatorio.", nameof(passwordHash));
        }

        PasswordVerificationResult result = Hasher.VerifyHashedPassword(User, passwordHash, plainPassword);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private sealed class PasswordHasherUser;
}
