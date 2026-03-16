using PlataformaECommerce.Domain.Entities.Products;
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
    private const int LongitudMinimaArea = 3;

    /// <summary>
    /// Longitud máxima permitida para el área del administrador.
    /// </summary>
    private const int LongitudMaximaArea = 60;

    /// <summary>
    /// Porcentaje máximo de descuento permitido para promociones comerciales.
    /// </summary>
    private const decimal PorcentajeMaximoDescuento = 90m;

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
        string area = "Operaciones")
        : base(nombre, correoElectronico, contrasenaHash)
    {
        Area = ValidarArea(area);
        Rol = RolUsuario.Administrador;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Área o dependencia organizacional a la que pertenece el administrador.
    /// </summary>
    public string Area { get; private set; } = string.Empty;

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

    /// <summary>
    /// Gestiona el inventario de un producto estableciendo un nuevo stock absoluto.
    /// </summary>
    /// <param name="producto">Producto sobre el cual se realizará la operación.</param>
    /// <param name="nuevoStock">Nuevo valor absoluto del inventario.</param>
    public void GestionarInventario(Producto producto, int nuevoStock)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.ActualizarStock(nuevoStock);
        MarcarActualizacion();
    }

    /// <summary>
    /// Incrementa el inventario de un producto en una cantidad específica.
    /// </summary>
    /// <param name="producto">Producto sobre el cual se realizará la operación.</param>
    /// <param name="cantidad">Cantidad a adicionar al inventario actual.</param>
    public void IncrementarInventario(Producto producto, int cantidad)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.IncrementarStock(cantidad);
        MarcarActualizacion();
    }

    /// <summary>
    /// Reduce el inventario de un producto en una cantidad específica.
    /// </summary>
    /// <param name="producto">Producto sobre el cual se realizará la operación.</param>
    /// <param name="cantidad">Cantidad a descontar del inventario actual.</param>
    public void ReducirInventario(Producto producto, int cantidad)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.DisminuirStock(cantidad);
        MarcarActualizacion();
    }

    /// <summary>
    /// Aplica una promoción a un producto reduciendo su precio de acuerdo con
    /// un porcentaje de descuento permitido por el dominio.
    /// </summary>
    /// <param name="producto">Producto sobre el cual se aplicará la promoción.</param>
    /// <param name="porcentajeDescuento">Porcentaje de descuento a aplicar.</param>
    public void AplicarPromocion(Producto producto, decimal porcentajeDescuento)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.ValidarDisponibilidad();

        decimal descuentoValidado = ValidarPorcentajeDescuento(porcentajeDescuento);
        decimal factorDescuento = ObtenerFactorDescuento(descuentoValidado);

        Money nuevoPrecio = producto.Precio * factorDescuento;

        if (!nuevoPrecio.IsPositive())
        {
            throw new ProductException("El precio resultante de la promoción debe ser mayor que cero.");
        }

        if (nuevoPrecio >= producto.Precio)
        {
            throw new ProductException("La promoción aplicada debe generar una reducción real en el precio del producto.");
        }

        producto.ActualizarPrecio(nuevoPrecio);
        MarcarActualizacion();
    }

    /// <summary>
    /// Activa un producto dentro del catálogo para habilitar su operación comercial.
    /// </summary>
    /// <param name="producto">Producto a activar.</param>
    public void ActivarProducto(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.Activar();
        MarcarActualizacion();
    }

    /// <summary>
    /// Desactiva un producto dentro del catálogo para impedir su operación comercial.
    /// </summary>
    /// <param name="producto">Producto a desactivar.</param>
    public void DesactivarProducto(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.Desactivar();
        MarcarActualizacion();
    }

    /// <summary>
    /// Marca un producto como destacado dentro del catálogo.
    /// </summary>
    /// <param name="producto">Producto a destacar.</param>
    public void DestacarProducto(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.MarcarComoDestacado();
        MarcarActualizacion();
    }

    /// <summary>
    /// Retira la marca de destacado de un producto dentro del catálogo.
    /// </summary>
    /// <param name="producto">Producto al que se le retirará la marca de destacado.</param>
    public void QuitarDestacadoProducto(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.QuitarDestacado();
        MarcarActualizacion();
    }

    /// <summary>
    /// Devuelve una representación legible y enriquecida del perfil del administrador.
    /// </summary>
    /// <returns>Cadena descriptiva con la información principal del administrador.</returns>
    public override string MostrarPerfil()
    {
        return $"{base.MostrarPerfil()} | Área: {Area}";
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
    /// Valida que el porcentaje de descuento se encuentre dentro del rango permitido.
    /// </summary>
    /// <param name="porcentajeDescuento">Porcentaje de descuento a validar.</param>
    /// <returns>Porcentaje válido y normalizado.</returns>
    private static decimal ValidarPorcentajeDescuento(decimal porcentajeDescuento)
    {
        if (porcentajeDescuento <= 0 || porcentajeDescuento > PorcentajeMaximoDescuento)
        {
            throw new ProductException($"El porcentaje de descuento debe ser mayor que cero y no superar el {PorcentajeMaximoDescuento}%.");
        }

        return decimal.Round(porcentajeDescuento, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Obtiene el factor multiplicador que debe aplicarse al precio
    /// para representar el descuento comercial.
    /// </summary>
    /// <param name="porcentajeDescuento">Porcentaje de descuento validado.</param>
    /// <returns>Factor decimal de descuento.</returns>
    private static decimal ObtenerFactorDescuento(decimal porcentajeDescuento)
    {
        decimal factorDescuento = 1m - (porcentajeDescuento / 100m);

        if (factorDescuento <= 0m)
        {
            throw new ProductException("El factor de descuento calculado no es válido.");
        }

        return factorDescuento;
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