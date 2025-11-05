using Cobrio.Application.DTOs.UsuarioEmpresa;
using Cobrio.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Apenas Admin pode gerenciar usuários
public class UsuarioEmpresaController : ControllerBase
{
    private readonly UsuarioEmpresaService _usuarioService;
    private readonly ILogger<UsuarioEmpresaController> _logger;

    public UsuarioEmpresaController(
        UsuarioEmpresaService usuarioService,
        ILogger<UsuarioEmpresaController> logger)
    {
        _usuarioService = usuarioService;
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

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var usuarioId))
        {
            throw new UnauthorizedAccessException("Usuário não identificado");
        }
        return usuarioId;
    }

    /// <summary>
    /// Lista todos os usuários da empresa
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioEmpresaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var usuarios = await _usuarioService.GetAllByEmpresaAsync(empresaId, cancellationToken);
            return Ok(usuarios);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return BadRequest(new { message = "Erro ao listar usuários" });
        }
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioEmpresaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var usuario = await _usuarioService.GetByIdAsync(empresaId, id, cancellationToken);
            return Ok(usuario);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário");
            return BadRequest(new { message = "Erro ao obter usuário" });
        }
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioEmpresaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CreateUsuarioEmpresaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var usuario = await _usuarioService.CreateAsync(empresaId, request, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário");
            return BadRequest(new { message = "Erro ao criar usuário" });
        }
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UsuarioEmpresaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateUsuarioEmpresaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var currentUserId = GetCurrentUserId();
            var usuario = await _usuarioService.UpdateAsync(empresaId, id, request, currentUserId, cancellationToken);
            return Ok(usuario);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário");
            return BadRequest(new { message = "Erro ao atualizar usuário" });
        }
    }

    /// <summary>
    /// Desativa um usuário (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var currentUserId = GetCurrentUserId();
            await _usuarioService.DeleteAsync(empresaId, id, currentUserId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar usuário");
            return BadRequest(new { message = "Erro ao deletar usuário" });
        }
    }

    /// <summary>
    /// Reseta a senha de um usuário
    /// </summary>
    [HttpPost("{id}/resetar-senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetarSenha(Guid id, [FromBody] ResetarSenhaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            await _usuarioService.ResetarSenhaAsync(empresaId, id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resetar senha");
            return BadRequest(new { message = "Erro ao resetar senha" });
        }
    }
}
