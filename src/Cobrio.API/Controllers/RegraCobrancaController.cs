using Cobrio.Application.DTOs.RegraCobranca;
using Cobrio.Application.DTOs.HistoricoImportacao;
using Cobrio.Application.Interfaces;
using Cobrio.Application.Services;
using Cobrio.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegraCobrancaController : ControllerBase
{
    private readonly IRegraCobrancaService _regraService;
    private readonly ExcelImportService _excelImportService;
    private readonly IHistoricoImportacaoRepository _historicoRepository;
    private readonly ILogger<RegraCobrancaController> _logger;

    public RegraCobrancaController(
        IRegraCobrancaService regraService,
        ExcelImportService excelImportService,
        IHistoricoImportacaoRepository historicoRepository,
        ILogger<RegraCobrancaController> logger)
    {
        _regraService = regraService;
        _excelImportService = excelImportService;
        _historicoRepository = historicoRepository;
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
    /// Lista todas as regras de cobrança
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RegraCobrancaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool? apenasAtivas = null)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();

            var regras = apenasAtivas == true
                ? await _regraService.GetRegrasAtivasAsync(empresaId)
                : await _regraService.GetAllAsync(empresaId);

            return Ok(regras);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar regras de cobrança");
            return BadRequest(new { message = "Erro ao listar regras de cobrança" });
        }
    }

    /// <summary>
    /// Obtém uma regra de cobrança por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RegraCobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var regra = await _regraService.GetByIdAsync(empresaId, id);
            return Ok(regra);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter regra de cobrança {RegraId}", id);
            return BadRequest(new { message = "Erro ao obter regra de cobrança" });
        }
    }

    /// <summary>
    /// Cria uma nova regra de cobrança
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegraCobrancaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CreateRegraCobrancaRequest request)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var regra = await _regraService.CreateAsync(empresaId, request);
            return CreatedAtAction(nameof(ObterPorId), new { id = regra.Id }, regra);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar regra de cobrança");
            return BadRequest(new { message = "Erro ao criar regra de cobrança" });
        }
    }

    /// <summary>
    /// Atualiza uma regra de cobrança existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RegraCobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateRegraCobrancaRequest request)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var regra = await _regraService.UpdateAsync(empresaId, id, request);
            return Ok(regra);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar regra de cobrança {RegraId}", id);
            return BadRequest(new { message = "Erro ao atualizar regra de cobrança" });
        }
    }

    /// <summary>
    /// Exclui uma regra de cobrança
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            await _regraService.DeleteAsync(empresaId, id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir regra de cobrança {RegraId}", id);
            return BadRequest(new { message = "Erro ao excluir regra de cobrança" });
        }
    }

    /// <summary>
    /// Ativa uma regra de cobrança
    /// </summary>
    [HttpPost("{id}/ativar")]
    [ProducesResponseType(typeof(RegraCobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(Guid id)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var regra = await _regraService.AtivarAsync(empresaId, id);
            return Ok(regra);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar regra de cobrança {RegraId}", id);
            return BadRequest(new { message = "Erro ao ativar regra de cobrança" });
        }
    }

    /// <summary>
    /// Desativa uma regra de cobrança
    /// </summary>
    [HttpPost("{id}/desativar")]
    [ProducesResponseType(typeof(RegraCobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var regra = await _regraService.DesativarAsync(empresaId, id);
            return Ok(regra);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar regra de cobrança {RegraId}", id);
            return BadRequest(new { message = "Erro ao desativar regra de cobrança" });
        }
    }

    /// <summary>
    /// Regenera o token de uma regra de cobrança
    /// </summary>
    [HttpPost("{id}/regenerar-token")]
    [ProducesResponseType(typeof(RegraCobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenerarToken(Guid id)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var regra = await _regraService.RegenerarTokenAsync(empresaId, id);
            return Ok(regra);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao regenerar token da regra {RegraId}", id);
            return BadRequest(new { message = "Erro ao regenerar token" });
        }
    }

    /// <summary>
    /// Baixa o modelo Excel para importação em massa
    /// </summary>
    [HttpGet("{id}/modelo-excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaixarModeloExcel(Guid id)
    {
        try
        {
            var excelBytes = await _excelImportService.GerarModeloExcelAsync(id);
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"modelo-cobranca-{id}.xlsx");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar modelo Excel para regra {RegraId}", id);
            return BadRequest(new { message = "Erro ao gerar modelo Excel" });
        }
    }

    /// <summary>
    /// Importa cobranças em massa a partir de arquivo Excel
    /// </summary>
    [HttpPost("{id}/importar-excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10_000_000)] // 10MB
    public async Task<IActionResult> ImportarExcel(Guid id, IFormFile arquivo, [FromQuery] bool envioImediato = false)
    {
        try
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest(new { message = "Nenhum arquivo enviado" });
            }

            if (!arquivo.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Apenas arquivos .xlsx são aceitos" });
            }

            using var stream = arquivo.OpenReadStream();
            var resultado = await _excelImportService.ImportarCobrancasAsync(id, stream, arquivo.FileName, envioImediato);

            if (resultado.Sucesso)
            {
                return Ok(resultado);
            }
            else
            {
                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao importar Excel para regra {RegraId}", id);
            return BadRequest(new { message = "Erro ao processar arquivo Excel" });
        }
    }

    /// <summary>
    /// Importa cobranças em massa a partir de JSON
    /// </summary>
    [HttpPost("{id}/importar-json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarJson(Guid id, [FromBody] List<Application.DTOs.Cobranca.CreateCobrancaRequest> cobrancas, [FromQuery] bool envioImediato = false)
    {
        try
        {
            if (cobrancas == null || !cobrancas.Any())
            {
                return BadRequest(new { message = "Nenhuma cobrança enviada" });
            }

            var resultado = await _excelImportService.ImportarCobrancasJsonAsync(id, cobrancas, envioImediato);

            if (resultado.Sucesso)
            {
                return Ok(resultado);
            }
            else
            {
                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao importar JSON para regra {RegraId}", id);
            return BadRequest(new { message = "Erro ao processar JSON" });
        }
    }

    /// <summary>
    /// Obtém o histórico de importações de uma regra de cobrança
    /// </summary>
    [HttpGet("{id}/historico-importacoes")]
    [ProducesResponseType(typeof(IEnumerable<HistoricoImportacaoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterHistoricoImportacoes(Guid id)
    {
        try
        {
            var historicos = await _historicoRepository.GetByRegraIdAsync(id);

            var response = historicos.Select(h =>
            {
                List<ErroImportacaoDto>? erros = null;
                if (!string.IsNullOrEmpty(h.ErrosJson))
                {
                    try
                    {
                        erros = JsonSerializer.Deserialize<List<ErroImportacaoDto>>(h.ErrosJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao deserializar erros do histórico {HistoricoId}", h.Id);
                    }
                }

                return new HistoricoImportacaoResponse
                {
                    Id = h.Id,
                    RegraCobrancaId = h.RegraCobrancaId,
                    NomeRegra = h.RegraCobranca?.Nome ?? string.Empty,
                    UsuarioId = h.UsuarioId,
                    NomeUsuario = h.Usuario?.Nome ?? "Sistema",
                    NomeArquivo = h.NomeArquivo,
                    DataImportacao = h.DataImportacao,
                    Origem = h.Origem,
                    OrigemDescricao = h.Origem.ToString(),
                    TotalLinhas = h.TotalLinhas,
                    LinhasProcessadas = h.LinhasProcessadas,
                    LinhasComErro = h.LinhasComErro,
                    Status = h.Status,
                    StatusDescricao = h.Status.ToString(),
                    Erros = erros
                };
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter histórico de importações para regra {RegraId}", id);
            return BadRequest(new { message = "Erro ao obter histórico de importações" });
        }
    }
}
