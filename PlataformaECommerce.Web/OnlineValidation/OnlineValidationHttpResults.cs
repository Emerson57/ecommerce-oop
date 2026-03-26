using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PlataformaECommerce.Web.OnlineValidation;

/// <summary>
/// Centraliza la construcción de respuestas HTTP para validaciones online del sitio.
/// </summary>
/// <remarks>
/// Este helper permite estandarizar el contrato entre Razor Pages y los consumidores AJAX,
/// diferenciando cancelaciones esperadas, respuestas funcionales y fallos temporales de infraestructura.
/// </remarks>
internal static class OnlineValidationHttpResults
{
    /// <summary>
    /// Construye una respuesta funcional de validación online.
    /// </summary>
    public static JsonResult Ok(
        string code,
        string message,
        bool isValid,
        bool? isAvailable = null)
    {
        return new JsonResult(new OnlineValidationResponse(code, message, isValid, isAvailable, false));
    }

    /// <summary>
    /// Construye una respuesta vacía para cancelaciones esperadas del request.
    /// </summary>
    public static NoContentResult Canceled()
    {
        return new NoContentResult();
    }

    /// <summary>
    /// Construye una respuesta de indisponibilidad temporal para validaciones online.
    /// </summary>
    public static ObjectResult ServiceUnavailable(string code, string message)
    {
        return new ObjectResult(new OnlineValidationResponse(code, message, false, null, true))
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };
    }
}

/// <summary>
/// Representa el contrato uniforme de respuesta para validaciones online.
/// </summary>
internal sealed record OnlineValidationResponse(
    string Code,
    string Message,
    bool IsValid,
    bool? IsAvailable,
    bool IsTransientFailure);
