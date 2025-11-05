using Cobrio.Application.DTOs.PlanoOferta;
using Cobrio.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanoOfertaController : ControllerBase
{
    private readonly IPlanoOfertaService _planoService;
    private readonly ILogger<PlanoOfertaController> _logger;

    public PlanoOfertaController(IPlanoOfertaService planoService, ILogger<PlanoOfertaController> logger)
    {
        _planoService = planoService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os planos de oferta
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlanoOfertaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool? apenasAtivos = null)
    {
        try
        {
            var planos = await _planoService.ListarAsync(apenasAtivos);
            return Ok(planos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar planos de oferta");
            return BadRequest(new { message = "Erro ao listar planos" });
        }
    }

    /// <summary>
    /// Obtém um plano de oferta por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlanoOfertaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var plano = await _planoService.ObterPorIdAsync(id);
            return Ok(plano);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter plano {PlanoId}", id);
            return BadRequest(new { message = "Erro ao obter plano" });
        }
    }

    /// <summary>
    /// Cria um novo plano de oferta
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlanoOfertaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CreatePlanoOfertaRequest request)
    {
        try
        {
            var plano = await _planoService.CriarAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = plano.Id }, plano);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar plano de oferta");
            return BadRequest(new { message = "Erro ao criar plano" });
        }
    }

    /// <summary>
    /// Atualiza um plano de oferta existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PlanoOfertaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdatePlanoOfertaRequest request)
    {
        try
        {
            var plano = await _planoService.AtualizarAsync(id, request);
            return Ok(plano);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar plano {PlanoId}", id);
            return BadRequest(new { message = "Erro ao atualizar plano" });
        }
    }

    /// <summary>
    /// Ativa um plano de oferta
    /// </summary>
    [HttpPatch("{id}/ativar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(Guid id)
    {
        try
        {
            await _planoService.AtivarAsync(id);
            return Ok(new { message = "Plano ativado com sucesso" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar plano {PlanoId}", id);
            return BadRequest(new { message = "Erro ao ativar plano" });
        }
    }

    /// <summary>
    /// Desativa um plano de oferta
    /// </summary>
    [HttpPatch("{id}/desativar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id)
    {
        try
        {
            await _planoService.DesativarAsync(id);
            return Ok(new { message = "Plano desativado com sucesso" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar plano {PlanoId}", id);
            return BadRequest(new { message = "Erro ao desativar plano" });
        }
    }

    /// <summary>
    /// Exclui um plano de oferta
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        try
        {
            await _planoService.ExcluirAsync(id);
            return Ok(new { message = "Plano excluído com sucesso" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir plano {PlanoId}", id);
            return BadRequest(new { message = "Erro ao excluir plano" });
        }
    }
}
