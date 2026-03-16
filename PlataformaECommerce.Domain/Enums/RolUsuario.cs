namespace PlataformaECommerce.Domain.Enums;

/// <summary>
/// Define los roles funcionales que puede asumir un usuario dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Este enumerador permite clasificar a los usuarios del sistema según su nivel de
/// responsabilidad, acceso y capacidades dentro de la plataforma.
/// 
/// Los roles definidos aquí forman parte del dominio de negocio y permiten establecer
/// reglas claras de autorización, comportamiento del sistema y control de operaciones.
/// 
/// Aunque el sistema pueda utilizar mecanismos de autenticación o autorización externos
/// (por ejemplo ASP.NET Identity o proveedores de identidad), este enumerador representa
/// la clasificación lógica utilizada dentro del dominio del negocio.
/// </remarks>
public enum RolUsuario
{
    /// <summary>
    /// Representa a un usuario cliente del sistema.
    /// </summary>
    /// <remarks>
    /// Un cliente puede:
    /// - Navegar el catálogo de productos.
    /// - Agregar productos al carrito.
    /// - Realizar pedidos.
    /// - Consultar el estado de sus pedidos.
    /// - Gestionar su perfil personal.
    /// </remarks>
    Cliente = 1,

    /// <summary>
    /// Representa a un usuario administrador de la plataforma.
    /// </summary>
    /// <remarks>
    /// Un administrador posee privilegios elevados dentro del sistema, incluyendo:
    /// - Gestión de productos.
    /// - Gestión de inventario.
    /// - Administración de pedidos.
    /// - Gestión de usuarios.
    /// - Supervisión del funcionamiento general del sistema.
    /// </remarks>
    Administrador = 2
}