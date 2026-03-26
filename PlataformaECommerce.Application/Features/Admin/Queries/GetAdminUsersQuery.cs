namespace PlataformaECommerce.Application.Features.Admin.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener el módulo administrativo de usuarios.
/// </summary>
/// <remarks>
/// Esta query encapsula los criterios mínimos del backoffice de usuarios, incluyendo
/// validación de acceso, metadatos de trazabilidad y la ventana temporal utilizada para
/// medir accesos recientes sobre cuentas administrativas y clientes.
/// </remarks>
public sealed class GetAdminUsersQuery
{
    private const int DefaultRecentAccessWindowInDays = 30;
    private const int MinRecentAccessWindowInDays = 1;
    private const int MaxRecentAccessWindowInDays = 365;

    /// <summary>
    /// Indica si la consulta debe exigir acceso de super usuario.
    /// </summary>
    public bool RequireSuperUserAccess { get; init; } = true;

    /// <summary>
    /// Indica si la respuesta debe limitarse a usuarios activos.
    /// </summary>
    public bool OnlyActiveUsers { get; init; }

    /// <summary>
    /// Indica si la respuesta debe limitarse únicamente a cuentas administrativas.
    /// </summary>
    public bool OnlyAdministrativeUsers { get; init; }

    /// <summary>
    /// Ventana temporal usada para calcular accesos recientes.
    /// </summary>
    public int RecentAccessWindowInDays { get; init; } = DefaultRecentAccessWindowInDays;

    /// <summary>
    /// Fecha UTC de referencia para construir la respuesta.
    /// </summary>
    public DateTime? ReferenceDateUtc { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que origina la consulta.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Nombre visible opcional del usuario que origina la consulta.
    /// </summary>
    public string? RequestedByUserName { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se solicita la consulta.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Obtiene la ventana temporal normalizada para accesos recientes.
    /// </summary>
    public int NormalizedRecentAccessWindowInDays
    {
        get
        {
            if (RecentAccessWindowInDays < MinRecentAccessWindowInDays)
            {
                return DefaultRecentAccessWindowInDays;
            }

            return RecentAccessWindowInDays > MaxRecentAccessWindowInDays
                ? MaxRecentAccessWindowInDays
                : RecentAccessWindowInDays;
        }
    }

    /// <summary>
    /// Devuelve una representación resumida de la consulta del backoffice de usuarios.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetAdminUsersQuery | RequireSuperUserAccess: {RequireSuperUserAccess} | OnlyActiveUsers: {OnlyActiveUsers} | RecentAccessWindowInDays: {NormalizedRecentAccessWindowInDays} | Source: {Source} | ExternalReference: {ExternalReference}";
    }
}
