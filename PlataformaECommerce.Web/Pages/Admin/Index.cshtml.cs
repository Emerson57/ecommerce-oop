using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Admin;

/// <summary>
/// Proporciona el dashboard inicial del backoffice administrativo.
/// </summary>
/// <remarks>
/// Esta página actúa como punto de entrada del panel interno y resume el acceso
/// operativo hacia auditoría, catálogo y futuros módulos administrativos.
/// </remarks>
[Authorize(
    Policy = AuthorizationPolicies.AdminOnly,
    AuthenticationSchemes = AuthorizationPolicies.AdminCookieScheme)]
public sealed class IndexModel : PageModel
{
    private readonly IAdminDashboardService _adminDashboardService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
    /// </summary>
    /// <param name="adminDashboardService">Servicio especializado del dashboard administrativo.</param>
    public IndexModel(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService ?? throw new ArgumentNullException(nameof(adminDashboardService));
    }

    /// <summary>
    /// Obtiene el nombre visible del administrador autenticado.
    /// </summary>
    public string DisplayName { get; private set; } = "Administrador";

    /// <summary>
    /// Obtiene el correo electrónico del administrador autenticado.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Obtiene el área organizacional del administrador actual.
    /// </summary>
    public string Area { get; private set; } = "Operaciones";

    /// <summary>
    /// Obtiene el rol funcional del usuario autenticado.
    /// </summary>
    public string Role { get; private set; } = "Administrador";

    /// <summary>
    /// Obtiene un valor que indica si la cuenta actual posee privilegios de super usuario.
    /// </summary>
    public bool IsSuperUser { get; private set; }

    /// <summary>
    /// Obtiene un valor que indica si la solicitud actual proviene de un usuario autenticado.
    /// </summary>
    public bool IsAuthenticated { get; private set; }

    /// <summary>
    /// Obtiene las métricas reales del dashboard administrativo.
    /// </summary>
    public AdminDashboardDto Dashboard { get; private set; } = new();

    /// <summary>
    /// Obtiene el mensaje de error funcional del dashboard cuando la consulta falla.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Inicializa el dashboard administrativo a partir de los claims del usuario actual y de las métricas operativas.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        IsAuthenticated = User.Identity?.IsAuthenticated == true;
        DisplayName = User.Identity?.Name ?? "Administrador";
        Email = User.FindFirstValue(ClaimTypes.Email);
        Area = User.FindFirst(SecurityClaimTypes.AdminArea)?.Value ?? "Operaciones";
        Role = User.FindFirstValue(SecurityClaimTypes.PrimaryRole)
            ?? User.FindFirstValue(ClaimTypes.Role)
            ?? "Administrador";
        IsSuperUser = bool.TryParse(User.FindFirstValue(SecurityClaimTypes.IsSuperUser), out bool isSuperUser) && isSuperUser;

        GetAdminDashboardQuery query = new()
        {
            RequestedByUserName = DisplayName,
            Source = "AdminPortal"
        };

        var result = await _adminDashboardService.GetDashboardAsync(query, cancellationToken);
        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            return;
        }

        Dashboard = result.Value;
    }
}
