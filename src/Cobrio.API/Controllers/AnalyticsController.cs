using Cobrio.Application.DTOs.Analytics;
using Cobrio.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    private Guid GetEmpresaClienteId()
    {
        var tenantId = HttpContext.Items["TenantId"]?.ToString();
        if (string.IsNullOrEmpty(tenantId) || !Guid.TryParse(tenantId, out var empresaId))
        {
            throw new UnauthorizedAccessException("Empresa não identificada");
        }
        return empresaId;
    }

    /// <summary>
    /// Obtém todos os dados analíticos do dashboard
    /// </summary>
    /// <param name="dias">Número de dias para análise (padrão: 30)</param>
    /// <param name="tipoRegua">Filtro de tipo de régua (opcional)</param>
    /// <param name="canal">Filtro de canal (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados completos de analytics do dashboard</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardAnalytics(
        [FromQuery] int dias = 30,
        [FromQuery] string? tipoRegua = null,
        [FromQuery] string? canal = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();

            // Validações
            if (dias < 1 || dias > 365)
            {
                return BadRequest(new { message = "O período deve estar entre 1 e 365 dias" });
            }

            var analytics = await _analyticsService.GetDashboardAnalyticsAsync(
                empresaId,
                dias,
                tipoRegua,
                canal,
                cancellationToken);

            return Ok(analytics);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Tentativa de acesso não autorizado ao dashboard de analytics");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados de analytics. Dias: {Dias}, TipoRegua: {TipoRegua}, Canal: {Canal}",
                dias, tipoRegua ?? "todos", canal ?? "todos");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao buscar dados de analytics. Por favor, tente novamente." });
        }
    }
}
