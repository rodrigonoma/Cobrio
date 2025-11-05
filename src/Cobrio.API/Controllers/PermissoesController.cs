using Cobrio.Application.DTOs.Permissao;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissoesController : ControllerBase
{
    private readonly IPermissaoService _permissaoService;
    private readonly ILogger<PermissoesController> _logger;

    public PermissoesController(
        IPermissaoService permissaoService,
        ILogger<PermissoesController> logger)
    {
        _permissaoService = permissaoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os módulos ativos do sistema
    /// </summary>
    [HttpGet("modulos")]
    public async Task<IActionResult> GetModulos(CancellationToken cancellationToken)
    {
        try
        {
            var modulos = await _permissaoService.GetModulosAtivosAsync(cancellationToken);

            var response = modulos.Select(m => new ModuloResponse
            {
                Id = m.Id,
                Nome = m.Nome,
                Chave = m.Chave,
                Descricao = m.Descricao,
                Icone = m.Icone,
                Rota = m.Rota,
                Ordem = m.Ordem,
                Ativo = m.Ativo
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter módulos ativos");
            return StatusCode(500, new { message = "Erro ao obter módulos" });
        }
    }

    /// <summary>
    /// Obtém todas as ações ativas do sistema
    /// </summary>
    [HttpGet("acoes")]
    public async Task<IActionResult> GetAcoes(CancellationToken cancellationToken)
    {
        try
        {
            var acoes = await _permissaoService.GetAcoesAtivasAsync(cancellationToken);

            var response = acoes.Select(a => new AcaoResponse
            {
                Id = a.Id,
                Nome = a.Nome,
                Chave = a.Chave,
                Descricao = a.Descricao,
                TipoAcao = a.TipoAcao,
                Ativa = a.Ativa
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ações ativas");
            return StatusCode(500, new { message = "Erro ao obter ações" });
        }
    }

    /// <summary>
    /// Obtém as permissões configuradas para um perfil específico
    /// </summary>
    [HttpGet("perfil/{perfil}")]
    public async Task<IActionResult> GetPermissoesPorPerfil(
        [FromRoute] PerfilUsuario perfil,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();

            var modulos = await _permissaoService.GetModulosAtivosAsync(cancellationToken);
            var acoes = await _permissaoService.GetAcoesAtivasAsync(cancellationToken);
            var permissoes = await _permissaoService.GetPermissoesByPerfilAsync(
                empresaId,
                perfil,
                cancellationToken);

            // Mapeamento de módulo → ações válidas (sincronizado com funcionalidades reais do frontend)
            var moduloAcoesMap = new Dictionary<string, string[]>
            {
                ["dashboard"] = new[] { "menu.view", "read" },
                ["assinaturas"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete" },
                ["planos"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "toggle" },
                ["financeiro"] = new[] { "menu.view", "read" },
                ["regras-cobranca"] = new[] { "menu.view", "read", "create", "update", "delete", "export", "import" },
                ["usuarios"] = new[] { "menu.view", "read", "create", "update", "delete", "reset-password" },
                ["relatorios"] = new[] { "menu.view", "read", "export" },
                ["permissoes"] = new[] { "menu.view", "read", "config-permissions" }
            };

            var response = new PermissaoPerfilResponse
            {
                PerfilUsuario = perfil,
                Modulos = modulos.Select(m => new ModuloPermissaoResponse
                {
                    ModuloId = m.Id,
                    ModuloNome = m.Nome,
                    ModuloChave = m.Chave,
                    ModuloIcone = m.Icone,
                    ModuloRota = m.Rota,
                    Acoes = acoes
                        .Where(a =>
                        {
                            // Filtrar apenas ações válidas para este módulo
                            if (!moduloAcoesMap.TryGetValue(m.Chave, out var acoesValidas))
                                return false;
                            return acoesValidas.Contains(a.Chave);
                        })
                        .Select(a =>
                        {
                            var permissao = permissoes.FirstOrDefault(p =>
                                p.ModuloId == m.Id && p.AcaoId == a.Id);

                            return new AcaoPermissaoResponse
                            {
                                AcaoId = a.Id,
                                AcaoNome = a.Nome,
                                AcaoChave = a.Chave,
                                TipoAcao = a.TipoAcao,
                                Permitido = permissao?.Permitido ?? false
                            };
                        }).ToList()
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter permissões do perfil {Perfil}", perfil);
            return StatusCode(500, new { message = "Erro ao obter permissões" });
        }
    }

    /// <summary>
    /// Configura as permissões de um perfil (apenas Proprietário)
    /// </summary>
    [HttpPost("configurar")]
    public async Task<IActionResult> ConfigurarPermissoes(
        [FromBody] ConfigurarPermissoesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();
            var usuarioId = GetCurrentUserId();

            // Verificar se o usuário é proprietário
            // Esta verificação deveria estar em um middleware ou atributo personalizado
            var ehProprietarioClaim = User.FindFirst("EhProprietario")?.Value;
            if (ehProprietarioClaim != "True")
            {
                return Forbid("Apenas o proprietário pode configurar permissões");
            }

            await _permissaoService.ConfigurarPermissoesAsync(
                empresaId,
                request.PerfilUsuario,
                request.Permissoes,
                usuarioId,
                cancellationToken);

            _logger.LogInformation(
                "Permissões configuradas para perfil {Perfil} por usuário {UsuarioId}",
                request.PerfilUsuario,
                usuarioId);

            return Ok(new { message = "Permissões configuradas com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar permissões");
            return StatusCode(500, new { message = "Erro ao configurar permissões" });
        }
    }

    /// <summary>
    /// Verifica se um perfil tem uma permissão específica
    /// </summary>
    [HttpGet("verificar")]
    public async Task<IActionResult> VerificarPermissao(
        [FromQuery] PerfilUsuario perfil,
        [FromQuery] string moduloChave,
        [FromQuery] string acaoChave,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaId = GetEmpresaClienteId();

            // Verificar se é proprietário
            var ehProprietarioClaim = User.FindFirst("EhProprietario")?.Value;
            var ehProprietario = ehProprietarioClaim == "True";

            // Se for proprietário, tem acesso total (incluindo módulo de Permissões)
            if (ehProprietario)
            {
                return Ok(new { permitido = true });
            }

            // Caso contrário, verificar permissões normais
            var temPermissao = await _permissaoService.TemPermissaoAsync(
                empresaId,
                perfil,
                moduloChave,
                acaoChave,
                cancellationToken);

            return Ok(new { permitido = temPermissao });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao verificar permissão. Perfil: {Perfil}, Módulo: {Modulo}, Ação: {Acao}",
                perfil,
                moduloChave,
                acaoChave);
            return StatusCode(500, new { message = "Erro ao verificar permissão" });
        }
    }

    private Guid GetEmpresaClienteId()
    {
        var empresaIdClaim = User.FindFirst("EmpresaClienteId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim) || !Guid.TryParse(empresaIdClaim, out var empresaId))
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
}
