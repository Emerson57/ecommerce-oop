using System.Globalization;
using System.Text;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Infrastructure.Services.Products;

/// <summary>
/// Proporciona una fábrica utilitaria para crear entidades del dominio en escenarios
/// controlados como pruebas, seed de datos o inicialización de objetos de ejemplo.
/// </summary>
/// <remarks>
/// La fábrica encapsula la construcción de agregados utilizando el modelo vigente del dominio,
/// evitando exponer a consumidores de infraestructura detalles repetitivos de creación como
/// normalización de slug, creación de value objects o composición de etiquetas.
/// </remarks>
public static class FabricaEntidades
{
    #region Constantes internas de fábrica

    /// <summary>
    /// Discriminador funcional para productos digitales.
    /// </summary>
    private const string TipoProductoDigital = "digital";

    /// <summary>
    /// Discriminador funcional para productos físicos.
    /// </summary>
    private const string TipoProductoFisico = "fisico";

    private const string MonedaPorDefecto = "COP";

    #endregion

    #region Creación de productos

    /// <summary>
    /// Crea una instancia válida de <see cref="ProductoDigital"/> a partir de datos primitivos.
    /// </summary>
    /// <param name="nombre">Nombre comercial del producto.</param>
    /// <param name="descripcion">Descripción comercial o funcional.</param>
    /// <param name="precio">Precio unitario del producto.</param>
    /// <param name="stock">Stock inicial del producto.</param>
    /// <param name="formatoArchivo">Formato principal del archivo digital.</param>
    /// <param name="tamanoMB">Tamaño del archivo digital.</param>
    /// <param name="sku">SKU opcional. Si no se suministra, se genera a partir del nombre.</param>
    /// <param name="slug">Slug opcional. Si no se suministra, se genera a partir del nombre.</param>
    /// <param name="requiereLicencia">Indica si el producto requiere licencia.</param>
    /// <param name="imagenPrincipalUrl">Imagen principal asociada.</param>
    /// <param name="categoriaId">Categoría principal opcional.</param>
    /// <param name="subcategoriaId">Subcategoría opcional.</param>
    /// <param name="etiquetas">Etiquetas comerciales opcionales.</param>
    public static ProductoDigital CrearProductoDigital(
        string nombre,
        string descripcion,
        decimal precio,
        int stock,
        string formatoArchivo,
        decimal? tamanoMB,
        string? sku = null,
        string? slug = null,
        bool requiereLicencia = false,
        string? imagenPrincipalUrl = null,
        Guid? categoriaId = null,
        Guid? subcategoriaId = null,
        IEnumerable<string>? etiquetas = null)
    {
        return new ProductoDigital(
            nombre,
            descripcion,
            CrearSku(sku, nombre),
            new Money(precio, MonedaPorDefecto),
            stock,
            CrearSlug(slug, nombre),
            imagenPrincipalUrl,
            categoriaId,
            subcategoriaId,
            CrearEtiquetas(etiquetas),
            formatoArchivo,
            tamanoMB,
            requiereLicencia);
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="ProductoFisico"/> a partir de datos primitivos.
    /// </summary>
    /// <param name="nombre">Nombre comercial del producto.</param>
    /// <param name="descripcion">Descripción comercial o funcional.</param>
    /// <param name="precio">Precio unitario del producto.</param>
    /// <param name="stock">Stock inicial del producto.</param>
    /// <param name="pesoKg">Peso del producto.</param>
    /// <param name="altoCm">Alto del producto.</param>
    /// <param name="anchoCm">Ancho del producto.</param>
    /// <param name="largoCm">Largo del producto.</param>
    /// <param name="sku">SKU opcional. Si no se suministra, se genera a partir del nombre.</param>
    /// <param name="slug">Slug opcional. Si no se suministra, se genera a partir del nombre.</param>
    /// <param name="requiereEnvio">Indica si el producto requiere envío.</param>
    /// <param name="imagenPrincipalUrl">Imagen principal asociada.</param>
    /// <param name="categoriaId">Categoría principal opcional.</param>
    /// <param name="subcategoriaId">Subcategoría opcional.</param>
    /// <param name="etiquetas">Etiquetas comerciales opcionales.</param>
    public static ProductoFisico CrearProductoFisico(
        string nombre,
        string descripcion,
        decimal precio,
        int stock,
        decimal pesoKg,
        decimal altoCm,
        decimal anchoCm,
        decimal largoCm,
        string? sku = null,
        string? slug = null,
        bool requiereEnvio = true,
        string? imagenPrincipalUrl = null,
        Guid? categoriaId = null,
        Guid? subcategoriaId = null,
        IEnumerable<string>? etiquetas = null)
    {
        return new ProductoFisico(
            nombre,
            descripcion,
            CrearSku(sku, nombre),
            new Money(precio, MonedaPorDefecto),
            stock,
            CrearSlug(slug, nombre),
            imagenPrincipalUrl,
            categoriaId,
            subcategoriaId,
            CrearEtiquetas(etiquetas),
            pesoKg,
            altoCm,
            anchoCm,
            largoCm,
            requiereEnvio);
    }

    #endregion

    #region Creación de usuarios

    /// <summary>
    /// Crea una instancia válida de <see cref="Cliente"/>.
    /// </summary>
    /// <param name="nombre">Nombre completo del cliente.</param>
    /// <param name="correo">Correo electrónico principal.</param>
    /// <param name="contrasenaHash">Hash de contraseña.</param>
    public static Cliente CrearCliente(
        string nombre,
        string correo,
        string contrasenaHash)
    {
        return new Cliente(nombre, new Email(correo), contrasenaHash);
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="Administrador"/>.
    /// </summary>
    /// <param name="nombre">Nombre completo del administrador.</param>
    /// <param name="correo">Correo electrónico principal.</param>
    /// <param name="contrasenaHash">Hash de contraseña.</param>
    /// <param name="area">Área organizacional.</param>
    public static Administrador CrearAdministrador(
        string nombre,
        string correo,
        string contrasenaHash,
        string area = "Operaciones")
    {
        return new Administrador(nombre, new Email(correo), contrasenaHash, area);
    }

    #endregion

    #region Factory genérico

    /// <summary>
    /// Crea un producto según el tipo funcional especificado.
    /// </summary>
    /// <param name="tipoProducto">Tipo funcional de producto.</param>
    /// <param name="nombre">Nombre comercial del producto.</param>
    /// <param name="descripcion">Descripción comercial o funcional.</param>
    /// <param name="precio">Precio unitario del producto.</param>
    /// <param name="stock">Stock inicial del producto.</param>
    /// <param name="parametrosExtra">Parámetros específicos según el tipo de producto.</param>
    /// <returns>Instancia concreta del producto solicitado.</returns>
    public static Producto CrearProductoPorTipo(
        string tipoProducto,
        string nombre,
        string descripcion,
        decimal precio,
        int stock,
        params object[] parametrosExtra)
    {
        if (string.IsNullOrWhiteSpace(tipoProducto))
        {
            throw new FactoryException("El tipo de producto es obligatorio.");
        }

        ArgumentNullException.ThrowIfNull(parametrosExtra);

        string tipoNormalizado = tipoProducto.Trim().ToLowerInvariant();

        return tipoNormalizado switch
        {
            TipoProductoDigital => CrearProductoDigitalPorParametros(nombre, descripcion, precio, stock, parametrosExtra),
            TipoProductoFisico => CrearProductoFisicoPorParametros(nombre, descripcion, precio, stock, parametrosExtra),
            _ => throw new EntidadNoSoportadaException(tipoProducto, "Producto")
        };
    }

    #endregion

    #region Métodos privados auxiliares

    private static ProductoDigital CrearProductoDigitalPorParametros(
        string nombre,
        string descripcion,
        decimal precio,
        int stock,
        object[] parametrosExtra)
    {
        if (parametrosExtra.Length < 2)
        {
            throw new FactoryException("ProductoDigital requiere los parámetros: formatoArchivo y tamanoMB.");
        }

        if (parametrosExtra[0] is not string formatoArchivo)
        {
            throw new FactoryException("El parámetro formatoArchivo debe ser de tipo string.");
        }

        if (parametrosExtra[1] is not decimal tamanoMB)
        {
            throw new FactoryException("El parámetro tamanoMB debe ser de tipo decimal.");
        }

        bool requiereLicencia = parametrosExtra.Length >= 3 && parametrosExtra[2] is bool licencia && licencia;

        return CrearProductoDigital(nombre, descripcion, precio, stock, formatoArchivo, tamanoMB, requiereLicencia: requiereLicencia);
    }

    private static ProductoFisico CrearProductoFisicoPorParametros(
        string nombre,
        string descripcion,
        decimal precio,
        int stock,
        object[] parametrosExtra)
    {
        if (parametrosExtra.Length < 4)
        {
            throw new FactoryException("ProductoFisico requiere los parámetros: pesoKg, altoCm, anchoCm, largoCm.");
        }

        if (parametrosExtra[0] is not decimal pesoKg)
        {
            throw new FactoryException("El parámetro pesoKg debe ser de tipo decimal.");
        }

        if (parametrosExtra[1] is not decimal altoCm)
        {
            throw new FactoryException("El parámetro altoCm debe ser de tipo decimal.");
        }

        if (parametrosExtra[2] is not decimal anchoCm)
        {
            throw new FactoryException("El parámetro anchoCm debe ser de tipo decimal.");
        }

        if (parametrosExtra[3] is not decimal largoCm)
        {
            throw new FactoryException("El parámetro largoCm debe ser de tipo decimal.");
        }

        bool requiereEnvio = parametrosExtra.Length < 5 || parametrosExtra[4] is not bool envio || envio;

        return CrearProductoFisico(nombre, descripcion, precio, stock, pesoKg, altoCm, anchoCm, largoCm, requiereEnvio: requiereEnvio);
    }

    private static Sku CrearSku(string? sku, string nombre)
    {
        string valor = string.IsNullOrWhiteSpace(sku)
            ? GenerarCodigo(nombre).ToUpperInvariant()
            : sku;

        return new Sku(valor);
    }

    private static string CrearSlug(string? slug, string nombre)
    {
        return string.IsNullOrWhiteSpace(slug)
            ? GenerarCodigo(nombre).ToLowerInvariant()
            : slug.Trim().ToLowerInvariant();
    }

    private static IReadOnlyCollection<EtiquetaProducto> CrearEtiquetas(IEnumerable<string>? etiquetas)
    {
        if (etiquetas is null)
        {
            return Array.Empty<EtiquetaProducto>();
        }

        return etiquetas
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => new EtiquetaProducto(value))
            .ToArray();
    }

    private static string GenerarCodigo(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new FactoryException("El texto base para generar código de producto es obligatorio.");
        }

        string textoNormalizado = NormalizarTextoParaCodigo(texto);
        StringBuilder builder = new();
        bool ultimoFueSeparador = false;

        foreach (char caracter in textoNormalizado)
        {
            if (EsCaracterPermitidoParaCodigo(caracter))
            {
                builder.Append(char.ToUpperInvariant(caracter));
                ultimoFueSeparador = false;
                continue;
            }

            if (ultimoFueSeparador)
            {
                continue;
            }

            builder.Append('-');
            ultimoFueSeparador = true;
        }

        string codigo = builder.ToString().Trim('-');

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new FactoryException("No fue posible generar un código válido para la entidad solicitada.");
        }

        return codigo;
    }

    private static string NormalizarTextoParaCodigo(string texto)
    {
        string textoDescompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
        StringBuilder builder = new(textoDescompuesto.Length);

        foreach (char caracter in textoDescompuesto)
        {
            UnicodeCategory categoriaUnicode = CharUnicodeInfo.GetUnicodeCategory(caracter);

            if (categoriaUnicode == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(caracter);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool EsCaracterPermitidoParaCodigo(char caracter)
    {
        return char.IsDigit(caracter)
            || caracter is >= 'A' and <= 'Z'
            || caracter is >= 'a' and <= 'z';
    }

    #endregion
}