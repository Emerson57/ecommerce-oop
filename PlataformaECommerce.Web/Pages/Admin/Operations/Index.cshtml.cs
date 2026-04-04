using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Web.Pages.Admin.Operations;

/// <summary>
/// Expone la ficha operativa del cliente activo, soporte y trazabilidad del backoffice.
/// </summary>
[Authorize(
    Policy = AuthorizationPolicies.AdminOnly,
    AuthenticationSchemes = AuthorizationPolicies.AdminCookieScheme)]
public sealed class IndexModel : PageModel
{
    private readonly ClientExperienceOptions _clientExperienceOptions;
    private readonly RequestCorrelationOptions _requestCorrelationOptions;
    private readonly IHostEnvironment _hostEnvironment;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(
        IOptions<ClientExperienceOptions> clientExperienceOptions,
        IOptions<RequestCorrelationOptions> requestCorrelationOptions,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(clientExperienceOptions);
        ArgumentNullException.ThrowIfNull(requestCorrelationOptions);

        _clientExperienceOptions = clientExperienceOptions.Value;
        _requestCorrelationOptions = requestCorrelationOptions.Value;
        _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
    }

    /// <summary>
    /// Nombre visible del cliente configurado en la instancia.
    /// </summary>
    public string StorefrontName { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre del backoffice configurado en la instancia.
    /// </summary>
    public string BackofficeName { get; private set; } = string.Empty;

    /// <summary>
    /// Identificador lógico del cliente activo.
    /// </summary>
    public string ClientId { get; private set; } = string.Empty;

    /// <summary>
    /// Correo principal de soporte para la operación actual.
    /// </summary>
    public string SupportEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Teléfono principal de soporte para la operación actual.
    /// </summary>
    public string SupportPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Horario operativo vigente.
    /// </summary>
    public string SupportHours { get; private set; } = string.Empty;

    /// <summary>
    /// SLA base de soporte informado para la instancia.
    /// </summary>
    public string SupportSla { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre del header de correlación expuesto por la plataforma.
    /// </summary>
    public string CorrelationHeaderName { get; private set; } = string.Empty;

    /// <summary>
    /// Identificador de correlación de la solicitud actual.
    /// </summary>
    public string CurrentCorrelationId { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre del ambiente de ejecución actual.
    /// </summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>
    /// Versión informacional de la aplicación desplegada.
    /// </summary>
    public string ApplicationVersion { get; private set; } = string.Empty;

    /// <summary>
    /// Fecha y hora UTC de consulta del panel operativo.
    /// </summary>
    public DateTime GeneratedAtUtc { get; private set; }

    /// <summary>
    /// Guías operativas disponibles para instalación, operación y soporte.
    /// </summary>
    public IReadOnlyCollection<SupportDocumentItem> SupportDocuments { get; private set; } = Array.Empty<SupportDocumentItem>();

    /// <summary>
    /// Carga el contexto operativo del cliente activo para soporte y trazabilidad.
    /// </summary>
    public void OnGet()
    {
        StorefrontName = _clientExperienceOptions.StorefrontName;
        BackofficeName = _clientExperienceOptions.BackofficeName;
        ClientId = _clientExperienceOptions.ClientId;
        SupportEmail = _clientExperienceOptions.SupportEmail;
        SupportPhone = _clientExperienceOptions.SupportPhone;
        SupportHours = _clientExperienceOptions.SupportHours;
        SupportSla = _clientExperienceOptions.SupportSla;
        CorrelationHeaderName = _requestCorrelationOptions.CorrelationHeaderName;
        CurrentCorrelationId = HttpContext.Items.TryGetValue(RequestCorrelationMiddleware.CorrelationIdItemKey, out object? correlationIdValue)
            ? Convert.ToString(correlationIdValue, CultureInfo.InvariantCulture) ?? HttpContext.TraceIdentifier
            : HttpContext.TraceIdentifier;
        EnvironmentName = _hostEnvironment.EnvironmentName;
        ApplicationVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(Program).Assembly.GetName().Version?.ToString()
            ?? "0.0.0";
        GeneratedAtUtc = DateTime.UtcNow;
        SupportDocuments =
        [
            new SupportDocumentItem("Instalación", "docs/INSTALLATION.md", "Prerequisitos, configuración segura, aplicación de migraciones y arranque controlado."),
            new SupportDocumentItem("Operación", "docs/OPERATIONS.md", "Playbook de health checks, dashboard, monitoreo y operación diaria del backoffice."),
            new SupportDocumentItem("Soporte", "docs/SUPPORT.md", "Guía de atención con correlación, auditoría y datos mínimos para incidentes."),
            new SupportDocumentItem("Changelog", "CHANGELOG.md", "Historial de releases, cambios comerciales y ajustes operativos por versión.")
        ];
    }

    /// <summary>
    /// Representa un documento de referencia para operación y soporte.
    /// </summary>
    public sealed record SupportDocumentItem(string Title, string RepositoryPath, string Description);
}
