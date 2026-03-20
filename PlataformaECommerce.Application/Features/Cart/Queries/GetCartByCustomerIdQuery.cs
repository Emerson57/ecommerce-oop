using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Cart.DTOs;

namespace PlataformaECommerce.Application.Features.Cart.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener el carrito de compras
/// asociado a un cliente específico.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del sistema,
/// correspondiente al caso de uso de consultar el carrito activo o disponible
/// de un cliente a partir de su identificador.
///
/// Su responsabilidad es transportar los datos mínimos necesarios para que
/// la capa Application recupere la información desde la fuente de datos,
/// la proyecte adecuadamente y retorne una respuesta desacoplada del dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="CartDto"/> cuando la ejecución es exitosa.
///
/// Esta consulta no debe contener lógica de negocio ni acceso a infraestructura;
/// dichas responsabilidades pertenecen al servicio de aplicación y a los componentes
/// especializados de la capa Application e Infrastructure.
/// </remarks>
public sealed class GetCartByCustomerIdQuery
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia vacía de la consulta.
    /// </summary>
    public GetCartByCustomerIdQuery()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la consulta con el identificador del cliente.
    /// </summary>
    /// <param name="customerId">Identificador único del cliente propietario del carrito.</param>
    public GetCartByCustomerIdQuery(Guid customerId)
    {
        CustomerId = customerId;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Identificador único del cliente cuyo carrito será consultado.
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// Indica si la consulta debe restringirse a carritos activos.
    /// </summary>
    /// <remarks>
    /// En la mayoría de escenarios del e-commerce, el interés principal
    /// es recuperar el carrito activo del cliente. Esta propiedad permite
    /// mantener ese comportamiento explícito y evolutivo.
    /// </remarks>
    public bool OnlyActiveCart { get; init; } = true;

    /// <summary>
    /// Indica si la consulta debe incluir información extendida del carrito
    /// cuando la implementación del servicio de aplicación así lo soporte.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite evolucionar la consulta sin romper su contrato,
    /// por ejemplo para controlar inclusión de más metadatos, proyecciones
    /// enriquecidas o información adicional del contexto de compra.
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
        return $"GetCartByCustomerIdQuery | CustomerId: {CustomerId} | OnlyActiveCart: {OnlyActiveCart} | IncludeExtendedData: {IncludeExtendedData} | RequestedByUserId: {RequestedByUserId}";
    }

    #endregion
}