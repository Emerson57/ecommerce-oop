using PlataformaECommerce.Application.Interfaces.Services.Common;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Proporciona acceso desacoplado al reloj del sistema para la capa de aplicación.
/// </summary>
/// <remarks>
/// Esta implementación traduce el tiempo real del entorno de ejecución a la abstracción
/// <see cref="IDateTimeProvider"/>, manteniendo la lógica de negocio independiente de APIs estáticas.
/// </remarks>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public DateTime Now => DateTime.Now;

    /// <inheritdoc />
    public DateOnly UtcToday => DateOnly.FromDateTime(UtcNow);

    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(Now);
}
