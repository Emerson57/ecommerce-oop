namespace PlataformaECommerce.Application.Features.Admin.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener la definición funcional del formulario de creación de administradores.
/// </summary>
/// <remarks>
/// Esta query encapsula el contexto mínimo requerido para que la capa Application entregue a la UI
/// las restricciones del caso de uso, preservando la validación de acceso y la trazabilidad del backoffice.
/// </remarks>
public sealed class GetAdminRegistrationDefinitionQuery
{
    /// <summary>
    /// Indica si la consulta debe exigir acceso autenticado de super usuario.
    /// </summary>
    public bool RequireSuperUserAccess { get; init; } = true;

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
    /// Canal de origen desde el cual se solicita la definición funcional.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Devuelve una representación resumida de la consulta.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetAdminRegistrationDefinitionQuery | RequireSuperUserAccess: {RequireSuperUserAccess} | Source: {Source} | ExternalReference: {ExternalReference}";
    }
}
