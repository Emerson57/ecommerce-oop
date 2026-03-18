namespace PlataformaECommerce.Application.Interfaces.Services.Auth;

/// <summary>
/// Define el contrato del servicio responsable de generar y verificar
/// hashes de contraseñas dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este servicio abstrae la lógica criptográfica asociada al tratamiento
/// seguro de contraseñas, evitando que la capa Application o el Domain
/// dependan directamente de algoritmos, librerías o implementaciones concretas.
///
/// Su responsabilidad incluye:
/// - generar un hash seguro a partir de una contraseña en texto plano,
/// - verificar si una contraseña suministrada coincide con un hash previamente generado,
/// - encapsular la estrategia de hashing adoptada por el sistema.
///
/// La implementación concreta de esta interfaz debe residir en la capa Infrastructure
/// y puede apoyarse en tecnologías o bibliotecas como:
/// - ASP.NET Core Identity PasswordHasher,
/// - BCrypt,
/// - PBKDF2,
/// - Argon2,
/// - u otros mecanismos seguros y vigentes.
///
/// Esta interfaz no debe almacenar contraseñas ni exponer detalles internos
/// del algoritmo utilizado. Su objetivo es mantener la seguridad desacoplada
/// y correctamente encapsulada.
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera un hash seguro a partir de una contraseña en texto plano.
    /// </summary>
    /// <param name="plainPassword">Contraseña en texto plano que será transformada en hash.</param>
    /// <returns>
    /// Cadena que representa el hash seguro de la contraseña.
    /// </returns>
    /// <remarks>
    /// La implementación concreta debe aplicar una estrategia de hashing robusta,
    /// incluyendo los mecanismos necesarios de salting y work factor según corresponda.
    /// </remarks>
    string HashPassword(string plainPassword);

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con un hash previamente generado.
    /// </summary>
    /// <param name="plainPassword">Contraseña en texto plano suministrada para validación.</param>
    /// <param name="passwordHash">Hash almacenado contra el cual se realizará la verificación.</param>
    /// <returns>
    /// <see langword="true"/> si la contraseña coincide con el hash;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    bool VerifyPassword(string plainPassword, string passwordHash);
}