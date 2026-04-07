namespace PlataformaECommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la proyección persistente de un usuario dentro de la capa Infrastructure.
/// </summary>
/// <remarks>
/// Esta entidad consolida en un único registro la información base compartida por todos
/// los usuarios del sistema y los datos especializados requeridos para sus variantes
/// concretas, permitiendo reconstruir agregados de dominio como <c>Cliente</c> y
/// <c>Administrador</c> sin acoplar el modelo del dominio a Entity Framework Core.
/// SuperUsuario y Administrador comparten esta misma estructura persistente y se
/// diferencian exclusivamente por el valor controlado de <see cref="Rol"/>.
/// </remarks>
public sealed class UserEntity : ITenantOwnedEntity
{
    /// <summary>
    /// Obtiene o establece el identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador lógico del tenant propietario del usuario.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre completo del usuario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el correo electrónico principal normalizado del usuario.
    /// </summary>
    public string CorreoElectronico { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el hash persistido de la contraseña del usuario.
    /// </summary>
    public string ContrasenaHash { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el rol funcional del usuario dentro del dominio como texto controlado.
    /// </summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece un valor que indica si el usuario se encuentra activo.
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si el correo del usuario fue confirmado.
    /// </summary>
    public bool CorreoConfirmado { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de creación del usuario en UTC.
    /// </summary>
    public DateTime FechaCreacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de última actualización relevante del usuario en UTC.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha del último acceso exitoso del usuario en UTC.
    /// </summary>
    public DateTime? FechaUltimoAccesoUtc { get; set; }

    /// <summary>
    /// Obtiene o establece el área organizacional asociada a cuentas administrativas.
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// Obtiene o establece el historial de compras serializado del cliente.
    /// </summary>
    public string? HistorialComprasSerializado { get; set; }

    /// <summary>
    /// Obtiene o establece las preferencias serializadas del cliente.
    /// </summary>
    public string? PreferenciasSerializadas { get; set; }
}
