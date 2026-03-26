namespace PlataformaECommerce.Application.Features.Admin.DTOs;

/// <summary>
/// Representa la respuesta consolidada del backoffice de usuarios.
/// </summary>
/// <remarks>
/// Este DTO agrupa métricas resumidas y la colección proyectada de usuarios del sistema,
/// permitiendo que la capa web renderice el módulo administrativo de usuarios sin depender
/// de entidades de dominio ni de detalles de persistencia.
/// </remarks>
public sealed class AdminUsersBackofficeDto
{
    /// <summary>
    /// Fecha y hora UTC en que fue generado el resumen.
    /// </summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>
    /// Identificador del usuario que originó la consulta, cuando se conoce.
    /// </summary>
    public Guid? GeneratedByUserId { get; init; }

    /// <summary>
    /// Nombre visible del usuario que originó la consulta, cuando se conoce.
    /// </summary>
    public string? GeneratedByUserName { get; init; }

    /// <summary>
    /// Canal de origen asociado a la consulta.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Fecha UTC inicial de la ventana usada para medir accesos recientes.
    /// </summary>
    public DateTime RecentAccessWindowStartUtc { get; init; }

    /// <summary>
    /// Cantidad total de usuarios incluidos en la respuesta.
    /// </summary>
    public int TotalUsers { get; init; }

    /// <summary>
    /// Cantidad de usuarios activos.
    /// </summary>
    public int ActiveUsers { get; init; }

    /// <summary>
    /// Cantidad de usuarios inactivos.
    /// </summary>
    public int InactiveUsers { get; init; }

    /// <summary>
    /// Cantidad de usuarios con correo confirmado.
    /// </summary>
    public int EmailConfirmedUsers { get; init; }

    /// <summary>
    /// Cantidad de usuarios habilitados para operar.
    /// </summary>
    public int EnabledUsers { get; init; }

    /// <summary>
    /// Cantidad total de clientes.
    /// </summary>
    public int TotalCustomers { get; init; }

    /// <summary>
    /// Cantidad total de cuentas administrativas, incluyendo super usuarios.
    /// </summary>
    public int TotalAdministrators { get; init; }

    /// <summary>
    /// Cantidad total de super usuarios.
    /// </summary>
    public int TotalSuperUsers { get; init; }

    /// <summary>
    /// Cantidad de usuarios con acceso reciente dentro de la ventana consultada.
    /// </summary>
    public int UsersWithRecentAccess { get; init; }

    /// <summary>
    /// Colección proyectada de usuarios visibles en el backoffice.
    /// </summary>
    public IReadOnlyCollection<AdminBackofficeUserDto> Users { get; init; } = Array.Empty<AdminBackofficeUserDto>();
}
