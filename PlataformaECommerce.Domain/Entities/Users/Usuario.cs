using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Users;

/// <summary>
/// Representa la entidad base de un usuario dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Esta clase abstrae el comportamiento común de los usuarios del sistema,
/// centralizando reglas relacionadas con identidad, datos de contacto,
/// estado operativo, seguridad básica, rol funcional y trazabilidad temporal.
/// 
/// El correo electrónico se representa mediante el Value Object <see cref="Email"/>,
/// lo que permite encapsular las reglas de validación estructural y normalización
/// dentro del modelo de dominio.
///
/// La entidad no almacena contraseñas en texto plano. En su lugar, conserva
/// únicamente un hash de contraseña que debe ser generado por una capa externa
/// especializada en autenticación o seguridad.
/// </remarks>
public abstract class Usuario : AggregateRoot
{
    #region Constantes de negocio

    /// <summary>
    /// Longitud mínima permitida para el nombre del usuario.
    /// </summary>
    public const int LongitudMinimaNombre = 3;

    /// <summary>
    /// Longitud máxima permitida para el nombre del usuario.
    /// </summary>
    public const int LongitudMaximaNombre = 100;

    /// <summary>
    /// Longitud mínima razonable para un hash de contraseña.
    /// </summary>
    public const int LongitudMinimaHashContrasena = 20;

    /// <summary>
    /// Longitud máxima soportada para el hash de contraseña persistido.
    /// </summary>
    public const int LongitudMaximaHashContrasena = 500;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor protegido sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    protected Usuario()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="Usuario"/> con la información base requerida.
    /// </summary>
    /// <param name="nombre">Nombre completo del usuario.</param>
    /// <param name="correoElectronico">Correo electrónico representado como Value Object.</param>
    /// <param name="contrasenaHash">Hash de la contraseña del usuario.</param>
    protected Usuario(
        string nombre,
        Email correoElectronico,
        string contrasenaHash)
    {
        InicializarAggregateRoot();
        Nombre = ValidarNombre(nombre);
        CorreoElectronico = ValidarCorreoElectronico(correoElectronico);
        ContrasenaHash = ValidarContrasenaHash(contrasenaHash);

        Activo = true;
        CorreoConfirmado = false;
        FechaUltimoAccesoUtc = null;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del usuario representado como Value Object.
    /// </summary>
    public Email CorreoElectronico { get; private set; } = null!;

    /// <summary>
    /// Hash de la contraseña del usuario.
    /// </summary>
    /// <remarks>
    /// Este valor nunca debe corresponder a una contraseña en texto plano.
    /// Su generación y verificación deben ser responsabilidad de una capa especializada
    /// en autenticación o seguridad.
    /// </remarks>
    public string ContrasenaHash { get; private set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario se encuentra activo dentro del sistema.
    /// </summary>
    public bool Activo { get; private set; }

    /// <summary>
    /// Indica si el correo electrónico del usuario ya fue confirmado.
    /// </summary>
    public bool CorreoConfirmado { get; private set; }

    /// <summary>
    /// Fecha y hora UTC del último acceso registrado del usuario.
    /// </summary>
    public DateTime? FechaUltimoAccesoUtc { get; private set; }

    /// <summary>
    /// Rol funcional del usuario dentro del dominio del e-commerce.
    /// </summary>
    public RolUsuario Rol { get; protected set; }

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Actualiza los datos básicos del usuario.
    /// </summary>
    /// <param name="nombre">Nuevo nombre del usuario.</param>
    /// <param name="correoElectronico">Nuevo correo electrónico del usuario.</param>
    public virtual void ActualizarDatosBasicos(string nombre, Email correoElectronico)
    {
        ArgumentNullException.ThrowIfNull(correoElectronico);

        string currentEmail = CorreoElectronico.Value;
        Nombre = ValidarNombre(nombre);
        CorreoElectronico = ValidarCorreoElectronico(correoElectronico);

        if (!string.Equals(currentEmail, CorreoElectronico.Value, StringComparison.OrdinalIgnoreCase))
        {
            CorreoConfirmado = false;
        }

        MarcarActualizacion();
    }

    /// <summary>
    /// Actualiza el hash de la contraseña del usuario.
    /// </summary>
    /// <param name="nuevoContrasenaHash">Nuevo hash de contraseña.</param>
    public void CambiarContrasenaHash(string nuevoContrasenaHash)
    {
        ContrasenaHash = ValidarContrasenaHash(nuevoContrasenaHash);
        MarcarActualizacion();
    }

    /// <summary>
    /// Marca el correo electrónico del usuario como confirmado.
    /// </summary>
    public void ConfirmarCorreoElectronico()
    {
        if (CorreoConfirmado)
        {
            return;
        }

        CorreoConfirmado = true;
        MarcarActualizacion();
    }

    /// <summary>
    /// Registra el último acceso exitoso del usuario en tiempo UTC.
    /// </summary>
    public void RegistrarAcceso()
    {
        FechaUltimoAccesoUtc = DateTime.UtcNow;
        MarcarActualizacion();
    }

    /// <summary>
    /// Activa el usuario dentro del sistema.
    /// </summary>
    public void Activar()
    {
        if (Activo)
        {
            return;
        }

        Activo = true;
        MarcarActualizacion();
    }

    /// <summary>
    /// Desactiva lógicamente el usuario dentro del sistema.
    /// </summary>
    public void Desactivar()
    {
        if (!Activo)
        {
            return;
        }

        Activo = false;
        MarcarActualizacion();
    }

    /// <summary>
    /// Determina si el usuario se encuentra habilitado para operar dentro del sistema.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el usuario está activo y tiene el correo confirmado;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool EstaHabilitado()
    {
        return Activo && CorreoConfirmado;
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida el nombre del usuario conforme a las reglas del dominio.
    /// </summary>
    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new UsuarioNoValidoException("El nombre del usuario es obligatorio.");
        }

        string nombreNormalizado = nombre.Trim();

        if (nombreNormalizado.Length < LongitudMinimaNombre)
        {
            throw new UsuarioNoValidoException($"El nombre del usuario debe tener al menos {LongitudMinimaNombre} caracteres.");
        }

        if (nombreNormalizado.Length > LongitudMaximaNombre)
        {
            throw new UsuarioNoValidoException($"El nombre del usuario no puede superar los {LongitudMaximaNombre} caracteres.");
        }

        return nombreNormalizado;
    }

    /// <summary>
    /// Valida el Value Object del correo electrónico.
    /// </summary>
    private static Email ValidarCorreoElectronico(Email correoElectronico)
    {
        if (correoElectronico is null)
        {
            throw new UsuarioNoValidoException("El correo electrónico del usuario es obligatorio.");
        }

        return correoElectronico;
    }

    /// <summary>
    /// Valida el hash de la contraseña del usuario.
    /// </summary>
    private static string ValidarContrasenaHash(string contrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(contrasenaHash))
        {
            throw new UsuarioNoValidoException("El hash de la contraseña del usuario es obligatorio.");
        }

        string hashNormalizado = contrasenaHash.Trim();

        if (hashNormalizado.Length < LongitudMinimaHashContrasena)
        {
            throw new UsuarioNoValidoException("El hash de la contraseña del usuario no cumple la longitud mínima esperada.");
        }

        if (hashNormalizado.Length > LongitudMaximaHashContrasena)
        {
            throw new UsuarioNoValidoException("El hash de la contraseña del usuario supera la longitud máxima soportada.");
        }

        return hashNormalizado;
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del usuario para trazabilidad y depuración.
    /// </summary>
    public override string ToString()
    {
        return $"{Nombre} ({CorreoElectronico}) - {Rol} | Activo: {Activo}";
    }

    #endregion
}