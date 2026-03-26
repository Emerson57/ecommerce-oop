using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Users;

/// <summary>
/// Representa a un administrador dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// El administrador es un tipo de usuario con responsabilidades operativas y de gestión
/// sobre el catálogo, inventario y estado comercial de los productos. Esta entidad
/// encapsula comportamientos propios del rol administrativo dentro del negocio,
/// permitiendo ejecutar acciones controladas sobre los recursos del sistema.
/// </remarks>
public sealed class Administrador : Usuario
{
    #region Constantes de negocio

    /// <summary>
    /// Longitud mínima permitida para el área del administrador.
    /// </summary>
    public const int LongitudMinimaArea = 3;

    /// <summary>
    /// Longitud máxima permitida para el área del administrador.
    /// </summary>
    public const int LongitudMaximaArea = 60;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private Administrador()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="Administrador"/> con la información base requerida.
    /// </summary>
    /// <param name="nombre">Nombre completo del administrador.</param>
    /// <param name="correoElectronico">Correo electrónico principal del administrador representado como Value Object.</param>
    /// <param name="contrasenaHash">Hash de la contraseña del administrador.</param>
    /// <param name="area">Área o dependencia organizacional a la que pertenece.</param>
    public Administrador(
        string nombre,
        Email correoElectronico,
        string contrasenaHash,
        string area = "Operaciones",
        RolUsuario rol = RolUsuario.Administrador)
        : base(nombre, correoElectronico, contrasenaHash)
    {
        Area = ValidarArea(area);
        Rol = ValidarRol(rol);
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Área o dependencia organizacional a la que pertenece el administrador.
    /// </summary>
    public string Area { get; private set; } = string.Empty;

    /// <summary>
    /// Indica si el administrador actual posee privilegios de super usuario.
    /// </summary>
    public bool EsSuperUsuario => Rol == RolUsuario.SuperUsuario;

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Actualiza el área organizacional del administrador.
    /// </summary>
    /// <param name="nuevaArea">Nueva área o dependencia del administrador.</param>
    public void ActualizarArea(string nuevaArea)
    {
        Area = ValidarArea(nuevaArea);
        MarcarActualizacion();
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida y normaliza el área organizacional del administrador.
    /// </summary>
    /// <param name="area">Área a validar.</param>
    /// <returns>Área normalizada y válida.</returns>
    private static string ValidarArea(string area)
    {
        if (string.IsNullOrWhiteSpace(area))
        {
            throw new UsuarioNoValidoException("El área del administrador es obligatoria.");
        }

        string areaNormalizada = area.Trim();

        if (areaNormalizada.Length < LongitudMinimaArea)
        {
            throw new UsuarioNoValidoException($"El área del administrador debe tener al menos {LongitudMinimaArea} caracteres.");
        }

        if (areaNormalizada.Length > LongitudMaximaArea)
        {
            throw new UsuarioNoValidoException($"El área del administrador no puede superar los {LongitudMaximaArea} caracteres.");
        }

        return areaNormalizada;
    }

    /// <summary>
    /// Valida que el rol asignado corresponda a una cuenta administrativa soportada.
    /// </summary>
    /// <param name="rol">Rol administrativo a validar.</param>
    /// <returns>Rol administrativo válido.</returns>
    private static RolUsuario ValidarRol(RolUsuario rol)
    {
        return rol is RolUsuario.Administrador or RolUsuario.SuperUsuario
            ? rol
            : throw new UsuarioNoValidoException("El rol asignado al administrador no es válido.");
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del administrador para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del administrador.</returns>
    public override string ToString()
    {
        return $"{Nombre} ({CorreoElectronico}) - {Rol} | Área: {Area} | Activo: {Activo}";
    }

    #endregion
}