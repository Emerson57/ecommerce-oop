namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa el error generado cuando la información o el estado de un usuario
/// no cumple con las reglas requeridas por el dominio.
/// </summary>
/// <remarks>
/// Esta excepción puede utilizarse en escenarios donde un usuario no tiene los datos
/// mínimos esperados, se encuentra en un estado inconsistente o no está autorizado
/// a ejecutar una operación desde la perspectiva del negocio.
/// </remarks>
public class UsuarioNoValidoException : UserException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UsuarioNoValidoException"/>
    /// con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Descripción del motivo por el cual el usuario no es válido.</param>
    public UsuarioNoValidoException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UsuarioNoValidoException"/>
    /// usando el identificador del usuario y la razón del error.
    /// </summary>
    /// <param name="userId">Identificador del usuario afectado.</param>
    /// <param name="motivo">Motivo de la invalidez detectada.</param>
    public UsuarioNoValidoException(Guid userId, string motivo)
        : base($"El usuario con identificador '{userId}' no es válido. Motivo: {motivo}.")
    {
        UserId = userId;
        Motivo = motivo;
    }

    /// <summary>
    /// Obtiene el identificador del usuario afectado, si fue suministrado.
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Obtiene el motivo funcional por el cual el usuario es considerado inválido.
    /// </summary>
    public string? Motivo { get; }
}