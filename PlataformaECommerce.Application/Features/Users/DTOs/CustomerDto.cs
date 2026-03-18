using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Users.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos de un cliente
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar información de clientes desde
/// la capa Application hacia capas superiores como:
/// - Web API,
/// - paneles administrativos,
/// - módulos de CRM,
/// - consultas de perfil,
/// - procesos de atención o seguimiento comercial.
///
/// Su propósito es desacoplar la representación expuesta del cliente
/// respecto de la entidad de dominio <c>Cliente</c>, evitando filtrar
/// directamente detalles internos del modelo.
///
/// Esta clase amplía la representación base de usuario con información
/// relevante del contexto comercial del cliente, como historial de compras
/// y preferencias registradas.
/// </remarks>
public sealed class CustomerDto
{
    #region Identificación básica

    /// <summary>
    /// Identificador único del cliente.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre completo o nombre visible del cliente.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del cliente.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    #endregion

    #region Información funcional

    /// <summary>
    /// Rol funcional del usuario dentro del sistema.
    /// </summary>
    /// <remarks>
    /// Para este DTO, el valor esperado normalmente será <see cref="RolUsuario.Cliente"/>.
    /// </remarks>
    public RolUsuario Role { get; init; } = RolUsuario.Cliente;

    /// <summary>
    /// Indica si el cliente se encuentra activo dentro del sistema.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el correo electrónico del cliente ya fue confirmado.
    /// </summary>
    public bool IsEmailConfirmed { get; init; }

    #endregion

    #region Información comercial del cliente

    /// <summary>
    /// Cantidad total de compras registradas para el cliente.
    /// </summary>
    public int TotalPurchases { get; init; }

    /// <summary>
    /// Colección de identificadores de pedidos registrados en el historial del cliente.
    /// </summary>
    public IReadOnlyCollection<Guid> PurchaseHistory { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Colección de preferencias declaradas por el cliente.
    /// </summary>
    public IReadOnlyCollection<string> Preferences { get; init; } = Array.Empty<string>();

    #endregion

    #region Información temporal

    /// <summary>
    /// Fecha y hora UTC en que fue creado el cliente.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización relevante del cliente.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC del último acceso registrado del cliente.
    /// </summary>
    public DateTime? LastAccessAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el cliente se encuentra habilitado para operar dentro del sistema.
    /// </summary>
    public bool IsEnabled => IsActive && IsEmailConfirmed;

    /// <summary>
    /// Indica si el cliente posee historial de compras registrado.
    /// </summary>
    public bool HasPurchases => TotalPurchases > 0;

    /// <summary>
    /// Indica si el cliente tiene preferencias registradas.
    /// </summary>
    public bool HasPreferences => Preferences.Count > 0;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO de cliente.
    /// </summary>
    /// <returns>Cadena representativa del cliente.</returns>
    public override string ToString()
    {
        return $"CustomerDto | Id: {Id} | Name: {Name} | Email: {Email} | Role: {Role} | Active: {IsActive} | EmailConfirmed: {IsEmailConfirmed} | TotalPurchases: {TotalPurchases}";
    }

    #endregion
}