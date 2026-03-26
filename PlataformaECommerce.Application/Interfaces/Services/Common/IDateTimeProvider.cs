namespace PlataformaECommerce.Application.Interfaces.Services.Common;

/// <summary>
/// Define el contrato del servicio responsable de proporcionar
/// información temporal dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este servicio abstrae el acceso al tiempo del sistema para evitar
/// dependencias directas con APIs estáticas como:
/// - <see cref="DateTime.UtcNow"/>
/// - <see cref="DateTime.Now"/>
/// - <see cref="DateOnly.FromDateTime(DateTime)"/>
///
/// Su propósito es mejorar:
/// - testabilidad,
/// - consistencia temporal,
/// - trazabilidad,
/// - manejo de expiraciones,
/// - auditoría,
/// - y desac acoplamiento respecto al reloj del sistema.
///
/// La implementación concreta de esta interfaz debe residir en la capa Infrastructure
/// y será responsable de traducir el tiempo real del sistema a una fuente controlada
/// para la capa Application.
///
/// En escenarios de pruebas, esta abstracción permite simular fechas y horas
/// sin modificar la lógica del dominio o de los casos de uso.
/// </remarks>
public interface IDateTimeProvider
{
    /// <summary>
    /// Obtiene la fecha y hora actual en formato UTC.
    /// </summary>
    /// <remarks>
    /// Esta propiedad debe considerarse como la referencia temporal principal
    /// para operaciones de negocio, persistencia, expiración y auditoría.
    /// </remarks>
    DateTime UtcNow { get; }

    /// <summary>
    /// Obtiene la fecha y hora actual en horario local del entorno de ejecución.
    /// </summary>
    /// <remarks>
    /// Esta propiedad puede resultar útil en escenarios de presentación,
    /// integración o adaptación a configuraciones regionales específicas.
    /// </remarks>
    DateTime Now { get; }

    /// <summary>
    /// Obtiene la fecha actual basada en el tiempo UTC.
    /// </summary>
    /// <remarks>
    /// Esta propiedad resulta útil cuando una operación necesita trabajar
    /// únicamente con la componente de fecha, sin depender de la hora.
    /// </remarks>
    DateOnly UtcToday { get; }

    /// <summary>
    /// Obtiene la fecha actual basada en el horario local del entorno de ejecución.
    /// </summary>
    /// <remarks>
    /// Esta propiedad resulta útil cuando la lógica necesita operar con la fecha local
    /// sin considerar la componente horaria.
    /// </remarks>
    DateOnly Today { get; }
}