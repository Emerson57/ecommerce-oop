using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Features.Products.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener el detalle de un producto
/// a partir de su identificador único.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del sistema,
/// correspondiente al caso de uso de consultar la información detallada
/// de un producto específico.
///
/// Su responsabilidad es transportar los datos mínimos necesarios para que
/// la capa Application recupere la información desde la fuente de datos,
/// la proyecte adecuadamente y retorne una respuesta desacoplada del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="ProductDetailDto"/> cuando la ejecución es exitosa.
///
/// Esta consulta no debe contener lógica de negocio ni acceso a infraestructura;
/// dichas responsabilidades pertenecen al servicio de aplicación y a los componentes
/// especializados de la capa Application e Infrastructure.
/// </remarks>
public sealed class GetProductByIdQuery
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia vacía de la consulta.
    /// </summary>
    public GetProductByIdQuery()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la consulta con el identificador del producto.
    /// </summary>
    /// <param name="productId">Identificador único del producto a consultar.</param>
    public GetProductByIdQuery(Guid productId)
    {
        ProductId = productId;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Identificador único del producto que será consultado.
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Indica si la consulta debe incluir información extendida o complementaria
    /// cuando la implementación del servicio de aplicación así lo soporte.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite evolucionar la consulta sin romper su contrato,
    /// por ejemplo para controlar carga de imágenes adicionales, categorías,
    /// metadatos o información ampliada del catálogo.
    /// </remarks>
    public bool IncludeExtendedData { get; init; } = true;

    /// <summary>
    /// Identificador opcional del usuario que solicita la consulta.
    /// </summary>
    /// <remarks>
    /// Puede utilizarse para trazabilidad, auditoría o adaptación contextual
    /// de la respuesta cuando la capa superior decida enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    /// <remarks>
    /// Puede representar un identificador de correlación, un ticket
    /// o una referencia funcional útil para observabilidad.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetProductByIdQuery | ProductId: {ProductId} | IncludeExtendedData: {IncludeExtendedData} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}