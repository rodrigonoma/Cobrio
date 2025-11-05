using Cobrio.Application.DTOs.Assinatura;
using Cobrio.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssinaturaController : ControllerBase
{
    private readonly IAssinaturaService _assinaturaService;
    private readonly ILogger<AssinaturaController> _logger;

    public AssinaturaController(IAssinaturaService assinaturaService, ILogger<AssinaturaController> logger)
    {
        _assinaturaService = assinaturaService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as assinaturas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssinaturaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50)
    {
        try
        {
            var assinaturas = await _assinaturaService.ListarAsync(pagina, tamanhoPagina);
            return Ok(assinaturas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar assinaturas");
            return BadRequest(new { message = "Erro ao listar assinaturas" });
        }
    }

    /// <summary>
    /// Obtém uma assinatura por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssinaturaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var assinatura = await _assinaturaService.ObterPorIdAsync(id);
            return Ok(assinatura);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter assinatura {AssinaturaId}", id);
            return BadRequest(new { message = "Erro ao obter assinatura" });
        }
    }

    /// <summary>
    /// Cria uma nova assinatura
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssinaturaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CreateAssinaturaRequest request)
    {
        try
        {
            var assinatura = await _assinaturaService.CriarAssinaturaAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = assinatura.Id }, assinatura);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar assinatura");
            return BadRequest(new { message = "Erro ao criar assinatura" });
        }
    }

    /// <summary>
    /// Atualiza uma assinatura existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AssinaturaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateAssinaturaRequest request)
    {
        try
        {
            var assinatura = await _assinaturaService.AtualizarAsync(id, request);
            return Ok(assinatura);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar assinatura {AssinaturaId}", id);
            return BadRequest(new { message = "Erro ao atualizar assinatura" });
        }
    }

    /// <summary>
    /// Cancela uma assinatura
    /// </summary>
    [HttpPost("{id}/cancelar")]
    [ProducesResponseType(typeof(AssinaturaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarAssinaturaRequest request)
    {
        try
        {
            var assinatura = await _assinaturaService.CancelarAsync(id, request);
            return Ok(assinatura);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar assinatura {AssinaturaId}", id);
            return BadRequest(new { message = "Erro ao cancelar assinatura" });
        }
    }

    /// <summary>
    /// Suspende uma assinatura
    /// </summary>
    [HttpPost("{id}/suspender")]
    [ProducesResponseType(typeof(AssinaturaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspender(Guid id, [FromBody] SuspenderRequest? request = null)
    {
        try
        {
            var assinatura = await _assinaturaService.SuspenderAsync(id, request?.Motivo);
            return Ok(assinatura);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao suspender assinatura {AssinaturaId}", id);
            return BadRequest(new { message = "Erro ao suspender assinatura" });
        }
    }

    /// <summary>
    /// Reativa uma assinatura suspensa
    /// </summary>
    [HttpPost("{id}/reativar")]
    [ProducesResponseType(typeof(AssinaturaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(Guid id)
    {
        try
        {
            var assinatura = await _assinaturaService.ReativarAsync(id);
            return Ok(assinatura);
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
            _logger.LogError(ex, "Erro ao reativar assinatura {AssinaturaId}", id);
            return BadRequest(new { message = "Erro ao reativar assinatura" });
        }
    }

    /// <summary>
    /// Altera o plano de uma assinatura (upgrade/downgrade)
    /// </summary>
    [HttpPost("{id}/alterar-plano")]
    [ProducesResponseType(typeof(AssinaturaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarPlano(Guid id, [FromBody] AlterarPlanoRequest request)
    {
        try
        {
            var assinatura = await _assinaturaService.AlterarPlanoAsync(id, request.NovoPlanoId);
            return Ok(assinatura);
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
            _logger.LogError(ex, "Erro ao alterar plano da assinatura {AssinaturaId}", id);
            return BadRequest(new { message = "Erro ao alterar plano" });
        }
    }
}

public class SuspenderRequest
{
    public string? Motivo { get; set; }
}

public class AlterarPlanoRequest
{
    public Guid NovoPlanoId { get; set; }
}
