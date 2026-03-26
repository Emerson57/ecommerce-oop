namespace PlataformaECommerce.Web.Services.Products;

/// <summary>
/// Representa el resultado del procesamiento de la imagen principal de un producto.
/// </summary>
public sealed record ProductImageProcessResult
{
    /// <summary>
    /// Indica si el procesamiento fue exitoso.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// URL final que debe persistirse para la imagen principal cuando el procesamiento es exitoso.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Mensaje funcional de error cuando el procesamiento falla.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Crea un resultado exitoso.
    /// </summary>
    public static ProductImageProcessResult Success(string? imageUrl)
        => new() { IsSuccess = true, ImageUrl = imageUrl };

    /// <summary>
    /// Crea un resultado fallido.
    /// </summary>
    public static ProductImageProcessResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
