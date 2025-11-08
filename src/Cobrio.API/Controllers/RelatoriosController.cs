using Cobrio.Application.DTOs.Relatorios;
using Cobrio.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatoriosController : ControllerBase
{
    private readonly RelatoriosService _relatoriosService;
    private readonly ILogger<RelatoriosController> _logger;

    public RelatoriosController(
        RelatoriosService relatoriosService,
        ILogger<RelatoriosController> logger)
    {
        _relatoriosService = relatoriosService;
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
    /// Obtém métricas gerais do período
    /// </summary>
    [HttpGet("metricas-gerais")]
    [ProducesResponseType(typeof(MetricasGeraisResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricasGerais(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var metricas = await _relatoriosService.GetMetricasGeraisAsync(empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(metricas);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar métricas gerais");
            return BadRequest(new { message = "Erro ao buscar métricas gerais" });
        }
    }

    /// <summary>
    /// Obtém envios por regra de cobrança
    /// </summary>
    [HttpGet("envios-por-regra")]
    [ProducesResponseType(typeof(List<EnvioPorRegraResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnviosPorRegra(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var envios = await _relatoriosService.GetEnviosPorRegraAsync(empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(envios);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar envios por regra");
            return BadRequest(new { message = "Erro ao buscar envios por regra" });
        }
    }

    /// <summary>
    /// Obtém status de cobranças
    /// </summary>
    [HttpGet("status-cobrancas")]
    [ProducesResponseType(typeof(List<StatusCobrancaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatusCobrancas(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var status = await _relatoriosService.GetStatusCobrancasAsync(empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(status);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar status de cobranças");
            return BadRequest(new { message = "Erro ao buscar status de cobranças" });
        }
    }

    /// <summary>
    /// Obtém evolução de cobranças
    /// </summary>
    [HttpGet("evolucao-cobrancas")]
    [ProducesResponseType(typeof(List<EvolucaoCobrancaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvolucaoCobrancas(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var evolucao = await _relatoriosService.GetEvolucaoCobrancasAsync(empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(evolucao);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evolução de cobranças");
            return BadRequest(new { message = "Erro ao buscar evolução de cobranças" });
        }
    }

    /// <summary>
    /// Obtém status de assinaturas
    /// </summary>
    [HttpGet("status-assinaturas")]
    [ProducesResponseType(typeof(StatusAssinaturasResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatusAssinaturas(CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var status = await _relatoriosService.GetStatusAssinaturasAsync(empresaId, cancellationToken);
            return Ok(status);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar status de assinaturas");
            return BadRequest(new { message = "Erro ao buscar status de assinaturas" });
        }
    }

    /// <summary>
    /// Obtém histórico de importações
    /// </summary>
    [HttpGet("historico-importacoes")]
    [ProducesResponseType(typeof(List<HistoricoImportacaoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistoricoImportacoes(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var historico = await _relatoriosService.GetHistoricoImportacoesAsync(empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(historico);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar histórico de importações");
            return BadRequest(new { message = "Erro ao buscar histórico de importações" });
        }
    }
}
