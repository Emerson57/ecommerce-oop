using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.DTOs;

namespace PlataformaECommerce.Application.Features.Auth.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener la información
/// del usuario autenticado actualmente.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del módulo
/// de autenticación y seguridad, correspondiente al caso de uso de consultar
/// el contexto actual del usuario autenticado.
///
/// Su propósito es transportar la información mínima necesaria para que
/// la capa Application recupere y proyecte el usuario autenticado hacia
/// un <see cref="CurrentUserDto"/>, desacoplado del dominio y de la infraestructura.
///
/// Esta clase no debe contener lógica de autorización, resolución de identidad
/// ni acceso a almacenamiento. Dichas responsabilidades pertenecen al servicio
/// de aplicación especializado y a los componentes auxiliares de Application.
/// </remarks>
public sealed class GetCurrentUserQuery
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia vacía de la consulta.
    /// </summary>
    public GetCurrentUserQuery()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la consulta con el identificador
    /// del usuario autenticado esperado.
    /// </summary>
    /// <param name="authenticatedUserId">Identificador del usuario autenticado.</param>
    public GetCurrentUserQuery(Guid authenticatedUserId)
    {
        AuthenticatedUserId = authenticatedUserId;
    }

    #endregion

    #region Identidad autenticada

    /// <summary>
    /// Identificador único del usuario autenticado cuya información se desea consultar.
    /// </summary>
    public Guid AuthenticatedUserId { get; init; }

    /// <summary>
    /// Nombre de usuario autenticado esperado, cuando la capa superior desee informarlo.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse como validación adicional de consistencia
    /// entre el contexto autenticado y los datos persistidos.
    /// </remarks>
    public string? AuthenticatedUserName { get; init; }

    #endregion

    #region Opciones de proyección

    /// <summary>
    /// Indica si la respuesta debe incluir permisos efectivos del usuario,
    /// cuando la implementación lo soporte.
    /// </summary>
    public bool IncludePermissions { get; init; } = true;

    /// <summary>
    /// Indica si la respuesta debe incluir roles asignados al usuario,
    /// cuando la implementación lo soporte.
    /// </summary>
    public bool IncludeRoles { get; init; } = true;

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Referencia funcional externa asociada a la consulta, cuando aplique.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se realiza la consulta.
    /// </summary>
    public string? Source { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetCurrentUserQuery | AuthenticatedUserId: {AuthenticatedUserId} | IncludeRoles: {IncludeRoles} | IncludePermissions: {IncludePermissions} | Source: {Source}";
    }

    #endregion
}