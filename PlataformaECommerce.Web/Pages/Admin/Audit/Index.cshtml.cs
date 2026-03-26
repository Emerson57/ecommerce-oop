using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Web.Authorization;

namespace PlataformaECommerce.Web.Pages.Admin.Audit
{
    /// <summary>
    /// Proporciona la visualización administrativa del rastro transversal de auditoría.
    /// </summary>
    /// <remarks>
    /// Esta página permite explorar eventos auditados del sistema aplicando filtros por
    /// agregado, módulo, actor, correlación y rango temporal, facilitando tareas de
    /// soporte, seguimiento operativo e investigación funcional.
    /// </remarks>
    [Authorize(
        Policy = AuthorizationPolicies.AdminOnly,
        AuthenticationSchemes = AuthorizationPolicies.AdminCookieScheme)]
    public sealed class IndexModel : PageModel
    {
        private const int MaxVisiblePageLinks = 5;
        private readonly IAuditApplicationService _auditApplicationService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="IndexModel"/>.
        /// </summary>
        /// <param name="auditApplicationService">Servicio público del módulo de auditoría.</param>
        public IndexModel(IAuditApplicationService auditApplicationService)
        {
            _auditApplicationService = auditApplicationService ?? throw new ArgumentNullException(nameof(auditApplicationService));
        }

        [BindProperty(SupportsGet = true)]
        public string? AggregateId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? AggregateType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Module { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Action { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PerformedBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CorrelationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FromUtc { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ToUtc { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Obtiene la colección de eventos de auditoría a presentar.
        /// </summary>
        public IReadOnlyCollection<AuditEntryDto> AuditItems { get; private set; } = Array.Empty<AuditEntryDto>();

        /// <summary>
        /// Obtiene el mensaje de error funcional asociado a la consulta cuando exista.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Obtiene la cantidad total de eventos recuperados.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Obtiene la cantidad total de páginas calculadas para la consulta actual.
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Obtiene un valor que indica si existe una página anterior disponible.
        /// </summary>
        public bool HasPreviousPage { get; private set; }

        /// <summary>
        /// Obtiene un valor que indica si existe una página siguiente disponible.
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        /// Obtiene la colección de números de página visibles en la navegación.
        /// </summary>
        public IReadOnlyCollection<int> VisiblePageNumbers { get; private set; } = Array.Empty<int>();

        /// <summary>
        /// Obtiene la posición inicial del conjunto actual de resultados dentro del total.
        /// </summary>
        public int FirstItemNumber => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

        /// <summary>
        /// Obtiene la posición final del conjunto actual de resultados dentro del total.
        /// </summary>
        public int LastItemNumber => TotalCount == 0 ? 0 : FirstItemNumber + AuditItems.Count - 1;

        /// <summary>
        /// Ejecuta la consulta del rastro de auditoría utilizando los filtros suministrados.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            if (!TryBuildQuery(out GetAuditTrailQuery? query, out string? validationMessage))
            {
                ErrorMessage = validationMessage;
                return;
            }

            var result = await _auditApplicationService.GetAuditTrailAsync(query!, cancellationToken);
            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                return;
            }

            AuditItems = result.Value.Items;
            TotalCount = result.Value.TotalCount;
            PageNumber = result.Value.PageNumber;
            PageSize = result.Value.PageSize;
            TotalPages = result.Value.TotalPages;
            HasPreviousPage = result.Value.HasPreviousPage;
            HasNextPage = result.Value.HasNextPage;
            VisiblePageNumbers = BuildVisiblePageNumbers(PageNumber, TotalPages);
        }

        private bool TryBuildQuery(out GetAuditTrailQuery? query, out string? validationMessage)
        {
            query = null;
            validationMessage = null;

            Guid? aggregateId = null;
            if (!string.IsNullOrWhiteSpace(AggregateId))
            {
                if (!Guid.TryParse(AggregateId.Trim(), out Guid parsedAggregateId))
                {
                    validationMessage = "El identificador del agregado debe ser un GUID válido.";
                    return false;
                }

                aggregateId = parsedAggregateId;
            }

            if (!TryParseUtcDate(FromUtc, out DateTime? fromUtc, out validationMessage))
            {
                return false;
            }

            if (!TryParseUtcDate(ToUtc, out DateTime? toUtc, out validationMessage))
            {
                return false;
            }

            query = new GetAuditTrailQuery
            {
                AggregateId = aggregateId,
                AggregateType = Normalize(AggregateType),
                Module = Normalize(Module),
                Action = Normalize(Action),
                PerformedBy = Normalize(PerformedBy),
                CorrelationId = Normalize(CorrelationId),
                FromUtc = fromUtc,
                ToUtc = toUtc,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            return true;
        }

        private static bool TryParseUtcDate(string? value, out DateTime? result, out string? validationMessage)
        {
            validationMessage = null;
            result = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (!DateTime.TryParse(
                    value.Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTime parsedValue))
            {
                validationMessage = "Las fechas del filtro deben tener un formato válido.";
                return false;
            }

            result = DateTime.SpecifyKind(parsedValue, DateTimeKind.Utc);
            return true;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static IReadOnlyCollection<int> BuildVisiblePageNumbers(int pageNumber, int totalPages)
        {
            if (totalPages <= 0)
            {
                return Array.Empty<int>();
            }

            int halfWindow = MaxVisiblePageLinks / 2;
            int start = Math.Max(1, pageNumber - halfWindow);
            int end = Math.Min(totalPages, start + MaxVisiblePageLinks - 1);

            if ((end - start + 1) < MaxVisiblePageLinks)
            {
                start = Math.Max(1, end - MaxVisiblePageLinks + 1);
            }

            return Enumerable.Range(start, end - start + 1).ToArray();
        }
    }
}
