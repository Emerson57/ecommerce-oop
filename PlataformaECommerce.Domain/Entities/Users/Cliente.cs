using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Users;

/// <summary>
/// Representa a un cliente dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// El cliente es un tipo de usuario orientado a la operación comercial del sistema.
/// Puede realizar compras, mantener un historial de pedidos y registrar preferencias
/// que resultan útiles para personalización, segmentación y experiencia de usuario.
/// 
/// Esta entidad hereda de <see cref="Usuario"/> e incorpora comportamiento específico
/// asociado al rol de cliente dentro de la plataforma.
/// </remarks>
public sealed class Cliente : Usuario
{
    #region Constantes de negocio

    /// <summary>
    /// Longitud mínima permitida para una preferencia del cliente.
    /// </summary>
    private const int LongitudMinimaPreferencia = 2;

    /// <summary>
    /// Longitud máxima permitida para una preferencia del cliente.
    /// </summary>
    private const int LongitudMaximaPreferencia = 50;

    #endregion

    #region Campos privados

    /// <summary>
    /// Historial interno de pedidos asociados al cliente.
    /// </summary>
    private readonly List<Guid> _historialCompras = new();

    /// <summary>
    /// Conjunto de preferencias declaradas por el cliente.
    /// </summary>
    private readonly HashSet<string> _preferencias = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private Cliente()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="Cliente"/> con la información base requerida.
    /// </summary>
    /// <param name="nombre">Nombre completo del cliente.</param>
    /// <param name="correoElectronico">Correo electrónico principal del cliente representado como Value Object.</param>
    /// <param name="contrasenaHash">Hash de la contraseña del cliente.</param>
    public Cliente(
        string nombre,
        Email correoElectronico,
        string contrasenaHash)
        : base(nombre, correoElectronico, contrasenaHash)
    {
        Rol = RolUsuario.Cliente;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Devuelve el historial de compras del cliente como colección de solo lectura.
    /// </summary>
    public IReadOnlyCollection<Guid> HistorialCompras => _historialCompras.AsReadOnly();

    /// <summary>
    /// Devuelve las preferencias registradas del cliente como colección de solo lectura.
    /// </summary>
    public IReadOnlyCollection<string> Preferencias => _preferencias;

    /// <summary>
    /// Obtiene la cantidad total de compras registradas en el historial del cliente.
    /// </summary>
    public int TotalCompras => _historialCompras.Count;

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Registra un pedido dentro del historial de compras del cliente.
    /// </summary>
    /// <param name="pedidoId">Identificador del pedido a registrar.</param>
    public void RegistrarCompra(Guid pedidoId)
    {
        if (pedidoId == Guid.Empty)
        {
            throw new UserException("El identificador del pedido no puede ser vacío.");
        }

        if (_historialCompras.Contains(pedidoId))
        {
            throw new UserException($"El pedido con identificador '{pedidoId}' ya se encuentra registrado en el historial del cliente.");
        }

        _historialCompras.Add(pedidoId);
        MarcarActualizacion();
    }

    /// <summary>
    /// Determina si el cliente ya tiene registrada una compra específica.
    /// </summary>
    /// <param name="pedidoId">Identificador del pedido a consultar.</param>
    /// <returns>
    /// <see langword="true"/> si el pedido ya existe en el historial del cliente;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool TieneCompraRegistrada(Guid pedidoId)
    {
        if (pedidoId == Guid.Empty)
        {
            return false;
        }

        return _historialCompras.Contains(pedidoId);
    }

    /// <summary>
    /// Devuelve el historial de compras del cliente en un formato legible.
    /// </summary>
    /// <returns>Cadena con el historial resumido de pedidos del cliente.</returns>
    public string VerHistorialCompras()
    {
        if (_historialCompras.Count == 0)
        {
            return "Historial vacío: el cliente aún no registra compras.";
        }

        return $"Historial de compras (IDs de pedidos): {string.Join(", ", _historialCompras)}";
    }

    /// <summary>
    /// Agrega una nueva preferencia al perfil del cliente.
    /// </summary>
    /// <param name="preferencia">Preferencia a registrar.</param>
    public void AgregarPreferencia(string preferencia)
    {
        string preferenciaNormalizada = ValidarPreferencia(preferencia);

        if (!_preferencias.Add(preferenciaNormalizada))
        {
            throw new UserException($"La preferencia '{preferenciaNormalizada}' ya existe para este cliente.");
        }

        MarcarActualizacion();
    }

    /// <summary>
    /// Elimina una preferencia existente del perfil del cliente.
    /// </summary>
    /// <param name="preferencia">Preferencia a eliminar.</param>
    public void EliminarPreferencia(string preferencia)
    {
        string preferenciaNormalizada = ValidarPreferencia(preferencia);

        if (!_preferencias.Remove(preferenciaNormalizada))
        {
            throw new UserException($"La preferencia '{preferenciaNormalizada}' no existe en el perfil del cliente.");
        }

        MarcarActualizacion();
    }

    /// <summary>
    /// Determina si el cliente tiene registrada una preferencia específica.
    /// </summary>
    /// <param name="preferencia">Preferencia a validar.</param>
    /// <returns>
    /// <see langword="true"/> si la preferencia existe en el perfil del cliente;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool TienePreferencia(string preferencia)
    {
        if (string.IsNullOrWhiteSpace(preferencia))
        {
            return false;
        }

        string preferenciaNormalizada = preferencia.Trim();
        return _preferencias.Contains(preferenciaNormalizada);
    }

    /// <summary>
    /// Elimina todas las preferencias registradas del cliente.
    /// </summary>
    public void LimpiarPreferencias()
    {
        if (_preferencias.Count == 0)
        {
            return;
        }

        _preferencias.Clear();
        MarcarActualizacion();
    }

    /// <summary>
    /// Devuelve una representación legible y enriquecida del perfil del cliente.
    /// </summary>
    /// <returns>Cadena descriptiva con la información principal del cliente.</returns>
    public override string MostrarPerfil()
    {
        string preferenciasTexto = _preferencias.Count == 0
            ? "Sin preferencias"
            : string.Join(", ", _preferencias.OrderBy(p => p));

        return $"{base.MostrarPerfil()} | Compras registradas: {TotalCompras} | Preferencias: {preferenciasTexto}";
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida y normaliza una preferencia del cliente.
    /// </summary>
    /// <param name="preferencia">Preferencia a validar.</param>
    /// <returns>Preferencia normalizada y válida.</returns>
    private static string ValidarPreferencia(string preferencia)
    {
        if (string.IsNullOrWhiteSpace(preferencia))
        {
            throw new UserException("La preferencia del cliente es obligatoria.");
        }

        string preferenciaNormalizada = preferencia.Trim();

        if (preferenciaNormalizada.Length < LongitudMinimaPreferencia)
        {
            throw new UserException($"La preferencia del cliente debe tener al menos {LongitudMinimaPreferencia} caracteres.");
        }

        if (preferenciaNormalizada.Length > LongitudMaximaPreferencia)
        {
            throw new UserException($"La preferencia del cliente no puede superar los {LongitudMaximaPreferencia} caracteres.");
        }

        return preferenciaNormalizada;
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del cliente para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del cliente.</returns>
    public override string ToString()
    {
        return $"{Nombre} ({CorreoElectronico}) - {Rol} | Compras: {TotalCompras} | Activo: {Activo}";
    }

    #endregion
}