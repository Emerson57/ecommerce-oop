using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.SaaS;
using PlataformaECommerce.Application.Interfaces.Services.Common;
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
    private readonly RequestCorrelationOptions _requestCorrelationOptions;
    private readonly ITenantCatalogService _tenantCatalogService;
    private readonly IHostEnvironment _hostEnvironment;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    public IndexModel(
        IOptions<RequestCorrelationOptions> requestCorrelationOptions,
        ITenantCatalogService tenantCatalogService,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(requestCorrelationOptions);

        _requestCorrelationOptions = requestCorrelationOptions.Value;
        _tenantCatalogService = tenantCatalogService ?? throw new ArgumentNullException(nameof(tenantCatalogService));
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
    /// Modo de aislamiento de datos actualmente implementado por la plataforma.
    /// </summary>
    public string DataIsolationMode { get; private set; } = string.Empty;

    /// <summary>
    /// Cantidad de tenants configurados en la instancia actual.
    /// </summary>
    public int ConfiguredTenantsCount { get; private set; }

    /// <summary>
    /// Definición efectiva del tenant activo.
    /// </summary>
    public TenantDefinition CurrentTenant { get; private set; } = new();

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
    public async Task OnGetAsync()
    {
        CurrentTenant = await _tenantCatalogService.GetCurrentTenantAsync().ConfigureAwait(false);
        ConfiguredTenantsCount = (await _tenantCatalogService.GetConfiguredTenantsAsync().ConfigureAwait(false)).Count;
        DataIsolationMode = _tenantCatalogService.DataIsolationMode;
        StorefrontName = CurrentTenant.StorefrontName;
        BackofficeName = CurrentTenant.BackofficeName;
        ClientId = CurrentTenant.TenantId;
        SupportEmail = CurrentTenant.SupportEmail;
        SupportPhone = CurrentTenant.SupportPhone;
        SupportHours = CurrentTenant.SupportHours;
        SupportSla = CurrentTenant.SupportSla;
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
            new SupportDocumentItem("SaaS", "docs/SAAS.md", "Modelo de tenants, aislamiento de datos, planes, features y aprovisionamiento inicial."),
            new SupportDocumentItem("Soporte", "docs/SUPPORT.md", "Guía de atención con correlación, auditoría y datos mínimos para incidentes."),
            new SupportDocumentItem("Changelog", "CHANGELOG.md", "Historial de releases, cambios comerciales y ajustes operativos por versión.")
        ];
    }

    /// <summary>
    /// Representa un documento de referencia para operación y soporte.
    /// </summary>
    public sealed record SupportDocumentItem(string Title, string RepositoryPath, string Description);
}
