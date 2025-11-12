using Cobrio.Application.DTOs.Relatorios;
using Cobrio.API.Services;
using Cobrio.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatoriosAvancadosController : ControllerBase
{
    private readonly RelatoriosAvancadosService _relatoriosService;
    private readonly ILogger<RelatoriosAvancadosController> _logger;

    public RelatoriosAvancadosController(
        RelatoriosAvancadosService relatoriosService,
        ILogger<RelatoriosAvancadosController> logger)
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

    // ========================================================================
    // RELATÓRIOS OPERACIONAIS
    // ========================================================================

    /// <summary>
    /// Dashboard operacional completo com KPIs principais
    /// </summary>
    [HttpGet("dashboard-operacional")]
    [ProducesResponseType(typeof(DashboardOperacionalResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardOperacional(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] Guid? regraCobrancaId,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();

            // Validar período máximo (1 ano)
            if ((dataFim - dataInicio).TotalDays > 365)
            {
                return BadRequest(new { message = "Período máximo permitido: 365 dias" });
            }

            var resultado = await _relatoriosService.GetDashboardOperacionalAsync(
                empresaId, dataInicio, dataFim, regraCobrancaId, cancellationToken);

            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dashboard operacional");
            return BadRequest(new { message = "Erro ao buscar dashboard operacional" });
        }
    }

    /// <summary>
    /// Execução de réguas por dia/canal
    /// </summary>
    [HttpGet("execucao-reguas")]
    [ProducesResponseType(typeof(List<ExecucaoReguaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExecucaoReguas(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] CanalNotificacao? canal,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetExecucaoReguasAsync(
                empresaId, dataInicio, dataFim, canal, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar execução de réguas");
            return BadRequest(new { message = "Erro ao buscar execução de réguas" });
        }
    }

    /// <summary>
    /// Entregas, falhas e pendências detalhadas
    /// </summary>
    [HttpGet("entregas-falhas")]
    [ProducesResponseType(typeof(EntregasFalhasResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntregasFalhas(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetEntregasFalhasAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar entregas e falhas");
            return BadRequest(new { message = "Erro ao buscar entregas e falhas" });
        }
    }

    /// <summary>
    /// Cobranças enviadas vs recebimentos
    /// </summary>
    [HttpGet("cobrancas-recebimentos")]
    [ProducesResponseType(typeof(List<CobrancasRecebimentosResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCobrancasRecebimentos(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetCobrancasRecebimentosAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cobranças x recebimentos");
            return BadRequest(new { message = "Erro ao buscar cobranças x recebimentos" });
        }
    }

    /// <summary>
    /// Valores recuperados por régua de cobrança
    /// </summary>
    [HttpGet("valores-por-regua")]
    [ProducesResponseType(typeof(List<ValoresPorReguaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetValoresPorRegua(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetValoresPorReguaAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar valores por régua");
            return BadRequest(new { message = "Erro ao buscar valores por régua" });
        }
    }

    /// <summary>
    /// Pagamentos por faixa de atraso
    /// </summary>
    [HttpGet("pagamentos-por-atraso")]
    [ProducesResponseType(typeof(List<PagamentosPorAtrasoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagamentosPorAtraso(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetPagamentosPorAtrasoAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pagamentos por atraso");
            return BadRequest(new { message = "Erro ao buscar pagamentos por atraso" });
        }
    }

    // ========================================================================
    // RELATÓRIOS GERENCIAIS
    // ========================================================================

    /// <summary>
    /// Conversão por régua e canal
    /// </summary>
    [HttpGet("conversao-por-canal")]
    [ProducesResponseType(typeof(List<ConversaoPorCanalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversaoPorCanal(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetConversaoPorCanalAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conversão por canal");
            return BadRequest(new { message = "Erro ao buscar conversão por canal" });
        }
    }

    /// <summary>
    /// ROI das réguas de cobrança por período
    /// </summary>
    [HttpGet("roi-reguas")]
    [ProducesResponseType(typeof(List<ROIReguasResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetROIReguas(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetROIReguasAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ROI de réguas");
            return BadRequest(new { message = "Erro ao buscar ROI de réguas" });
        }
    }

    /// <summary>
    /// Evolução mensal de performance
    /// </summary>
    [HttpGet("evolucao-mensal")]
    [ProducesResponseType(typeof(List<EvolucaoMensalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvolucaoMensal(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetEvolucaoMensalAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evolução mensal");
            return BadRequest(new { message = "Erro ao buscar evolução mensal" });
        }
    }

    /// <summary>
    /// Melhor horário e dia para envio de cobranças
    /// </summary>
    [HttpGet("melhor-horario-envio")]
    [ProducesResponseType(typeof(MelhorHorarioEnvioResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMelhorHorarioEnvio(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetMelhorHorarioEnvioAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar melhor horário de envio");
            return BadRequest(new { message = "Erro ao buscar melhor horário de envio" });
        }
    }

    /// <summary>
    /// Redução de inadimplência com uso de réguas
    /// </summary>
    [HttpGet("reducao-inadimplencia")]
    [ProducesResponseType(typeof(ReducaoInadimplenciaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReducaoInadimplencia(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetReducaoInadimplenciaAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar redução de inadimplência");
            return BadRequest(new { message = "Erro ao buscar redução de inadimplência" });
        }
    }

    // ========================================================================
    // RELATÓRIOS HÍBRIDOS (OMNICHANNEL)
    // ========================================================================

    /// <summary>
    /// Tempo médio entre envio e pagamento
    /// </summary>
    [HttpGet("tempo-envio-pagamento")]
    [ProducesResponseType(typeof(List<TempoEnvioPagamentoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTempoEnvioPagamento(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetTempoEnvioPagamentoAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tempo envio → pagamento");
            return BadRequest(new { message = "Erro ao buscar tempo envio → pagamento" });
        }
    }

    /// <summary>
    /// Comparativo omnichannel vs single-channel
    /// </summary>
    [HttpGet("comparativo-omnichannel")]
    [ProducesResponseType(typeof(List<ComparativoOmnichannelResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComparativoOmnichannel(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var resultado = await _relatoriosService.GetComparativoOmnichannelAsync(
                empresaId, dataInicio, dataFim, cancellationToken);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar comparativo omnichannel");
            return BadRequest(new { message = "Erro ao buscar comparativo omnichannel" });
        }
    }

    // ========================================================================
    // RELATÓRIO DE CONSUMO
    // ========================================================================

    /// <summary>
    /// Dashboard de consumo de canais (Email, SMS, WhatsApp)
    /// </summary>
    [HttpGet("dashboard-consumo")]
    [ProducesResponseType(typeof(DashboardConsumoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardConsumo(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] CanalNotificacao? canal,
        [FromQuery] Guid? usuarioId,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();

            // Validar período máximo (1 ano)
            if ((dataFim - dataInicio).TotalDays > 365)
            {
                return BadRequest(new { message = "Período máximo permitido: 365 dias" });
            }

            var resultado = await _relatoriosService.GetDashboardConsumoAsync(
                empresaId, dataInicio, dataFim, canal, usuarioId, cancellationToken);

            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dashboard de consumo");
            return BadRequest(new { message = "Erro ao buscar dashboard de consumo" });
        }
    }
}
