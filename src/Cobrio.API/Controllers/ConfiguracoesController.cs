using Cobrio.Application.DTOs.Configuracoes;
using Cobrio.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracoesController : ControllerBase
{
    private readonly IEmpresaClienteRepository _empresaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfiguracoesController> _logger;

    public ConfiguracoesController(
        IEmpresaClienteRepository empresaRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfiguracoesController> logger)
    {
        _empresaRepository = empresaRepository;
        _unitOfWork = unitOfWork;
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
    /// Obtém as configurações de email da empresa
    /// </summary>
    [HttpGet("email")]
    [ProducesResponseType(typeof(EmailConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterConfigEmail()
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var empresa = await _empresaRepository.GetByIdAsync(empresaId);

            if (empresa == null)
                return NotFound("Empresa não encontrada");

            var response = new EmailConfigResponse
            {
                EmailRemetente = empresa.EmailRemetente,
                NomeRemetente = empresa.NomeRemetente,
                EmailReplyTo = empresa.EmailReplyTo
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configurações de email");
            return StatusCode(500, "Erro ao obter configurações");
        }
    }

    /// <summary>
    /// Atualiza as configurações de email da empresa
    /// </summary>
    [HttpPut("email")]
    [ProducesResponseType(typeof(EmailConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarConfigEmail([FromBody] EmailConfigRequest request)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var empresa = await _empresaRepository.GetByIdAsync(empresaId);

            if (empresa == null)
                return NotFound("Empresa não encontrada");

            // Atualizar configurações de email
            empresa.ConfigurarEmail(
                request.EmailRemetente,
                request.NomeRemetente,
                request.EmailReplyTo
            );

            _empresaRepository.Update(empresa);
            await _unitOfWork.CommitAsync();

            var response = new EmailConfigResponse
            {
                EmailRemetente = empresa.EmailRemetente,
                NomeRemetente = empresa.NomeRemetente,
                EmailReplyTo = empresa.EmailReplyTo
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validação falhou ao atualizar configurações de email");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar configurações de email");
            return StatusCode(500, "Erro ao atualizar configurações");
        }
    }
}
