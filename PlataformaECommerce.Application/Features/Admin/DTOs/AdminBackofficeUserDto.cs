using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Admin.DTOs;

/// <summary>
/// Representa la fila resumida de un usuario dentro del backoffice administrativo.
/// </summary>
/// <remarks>
/// Este DTO desacopla la visualización del módulo de usuarios respecto de las entidades
/// de dominio, exponiendo únicamente los datos necesarios para supervisión operativa,
/// segmentación por rol y verificación rápida del estado de habilitación.
/// </remarks>
public sealed class AdminBackofficeUserDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre visible del usuario.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del usuario.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Rol funcional del usuario dentro del sistema.
    /// </summary>
    public RolUsuario Role { get; init; }

    /// <summary>
    /// Indica si la cuenta corresponde a una identidad administrativa.
    /// </summary>
    public bool IsAdministrative { get; init; }

    /// <summary>
    /// Indica si la cuenta corresponde a un super usuario.
    /// </summary>
    public bool IsSuperUser { get; init; }

    /// <summary>
    /// Indica si la cuenta se encuentra activa.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el correo electrónico de la cuenta fue confirmado.
    /// </summary>
    public bool IsEmailConfirmed { get; init; }

    /// <summary>
    /// Indica si la cuenta se encuentra plenamente habilitada para operar.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Área organizacional de la cuenta administrativa, cuando aplique.
    /// </summary>
    public string? Area { get; init; }

    /// <summary>
    /// Fecha de creación de la cuenta en UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha de la última actualización relevante de la cuenta en UTC.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Fecha del último acceso registrado en UTC.
    /// </summary>
    public DateTime? LastAccessAtUtc { get; init; }
}
