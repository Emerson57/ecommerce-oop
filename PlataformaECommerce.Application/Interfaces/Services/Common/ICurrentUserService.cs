namespace PlataformaECommerce.Application.Interfaces.Services.Common;

/// <summary>
/// Define el contrato del servicio responsable de exponer la información
/// del usuario actualmente autenticado dentro del contexto de ejecución.
/// </summary>
/// <remarks>
/// Este servicio abstrae el acceso al usuario actual para que la capa Application
/// no dependa directamente de tecnologías o mecanismos concretos como:
/// - HttpContext,
/// - ClaimsPrincipal,
/// - JWT,
/// - cookies de autenticación,
/// - middleware web,
/// - o frameworks de transporte.
///
/// Su propósito es permitir que comandos, consultas, servicios de aplicación
/// y componentes de orquestación puedan conocer de forma desacoplada:
/// - quién ejecuta la operación,
/// - si está autenticado,
/// - qué rol posee,
/// - y qué información contextual de identidad está disponible.
///
/// La implementación concreta de esta interfaz debe residir en la capa Infrastructure
/// o Web, adaptando la fuente real de identidad hacia una representación estable
/// y segura para la capa Application.
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtiene el identificador del usuario autenticado actual.
    /// </summary>
    /// <remarks>
    /// Debe retornar <see langword="null"/> cuando no exista un usuario autenticado
    /// dentro del contexto de ejecución actual.
    /// </remarks>
    Guid? UserId { get; }

    /// <summary>
    /// Obtiene el nombre completo o nombre visible del usuario autenticado actual.
    /// </summary>
    /// <remarks>
    /// Debe retornar <see langword="null"/> cuando no exista un usuario autenticado
    /// o cuando la información no esté disponible en el contexto actual.
    /// </remarks>
    string? UserName { get; }

    /// <summary>
    /// Obtiene el correo electrónico del usuario autenticado actual.
    /// </summary>
    /// <remarks>
    /// Debe retornar <see langword="null"/> cuando no exista un usuario autenticado
    /// o cuando la información no esté disponible en el contexto actual.
    /// </remarks>
    string? Email { get; }

    /// <summary>
    /// Obtiene el rol principal del usuario autenticado actual.
    /// </summary>
    /// <remarks>
    /// Debe retornar <see langword="null"/> cuando no exista un usuario autenticado
    /// o cuando el rol no esté presente dentro del contexto actual.
    /// </remarks>
    string? Role { get; }

    /// <summary>
    /// Indica si existe un usuario autenticado en el contexto actual.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Indica si el usuario autenticado actual posee el rol especificado.
    /// </summary>
    /// <param name="role">Rol a validar.</param>
    /// <returns>
    /// <see langword="true"/> si el usuario actual posee el rol indicado;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    bool IsInRole(string role);

    /// <summary>
    /// Obtiene el valor de un claim específico del usuario actual.
    /// </summary>
    /// <param name="claimType">Tipo de claim a consultar.</param>
    /// <returns>
    /// El valor del claim si existe; en caso contrario, <see langword="null"/>.
    /// </returns>
    string? GetClaimValue(string claimType);

    /// <summary>
    /// Obtiene todos los valores asociados a un tipo de claim específico del usuario actual.
    /// </summary>
    /// <param name="claimType">Tipo de claim a consultar.</param>
    /// <returns>
    /// Una colección de valores asociados al claim solicitado.
    /// Si no existen valores, debe retornar una colección vacía.
    /// </returns>
    IReadOnlyCollection<string> GetClaimValues(string claimType);
}