namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa el error producido cuando se solicita crear o procesar un tipo de entidad no soportado.
/// </summary>
/// <remarks>
/// Esta excepción permite expresar de forma explícita que un discriminador funcional recibido
/// no corresponde a ningún tipo admitido por el modelo actual.
/// </remarks>
public sealed class EntidadNoSoportadaException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="EntidadNoSoportadaException"/>.
    /// </summary>
    /// <param name="tipoEntidad">Tipo recibido que no se encuentra soportado.</param>
    /// <param name="familiaEntidad">Familia o agrupación funcional esperada.</param>
    public EntidadNoSoportadaException(string tipoEntidad, string familiaEntidad)
        : base($"El tipo '{tipoEntidad}' no se encuentra soportado para la familia de entidades '{familiaEntidad}'.")
    {
        TipoEntidad = tipoEntidad;
        FamiliaEntidad = familiaEntidad;
    }

    /// <summary>
    /// Tipo no soportado recibido en la operación.
    /// </summary>
    public string TipoEntidad { get; }

    /// <summary>
    /// Familia de entidades para la cual se evaluó el tipo recibido.
    /// </summary>
    public string FamiliaEntidad { get; }
}