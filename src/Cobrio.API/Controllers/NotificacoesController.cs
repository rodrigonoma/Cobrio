using Cobrio.Application.DTOs.HistoricoNotificacao;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cobrio.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificacoesController : ControllerBase
{
    private readonly IHistoricoNotificacaoRepository _historicoRepository;
    private readonly IUsuarioEmpresaRepository _usuarioRepository;
    private readonly ILogger<NotificacoesController> _logger;

    public NotificacoesController(
        IHistoricoNotificacaoRepository historicoRepository,
        IUsuarioEmpresaRepository usuarioRepository,
        ILogger<NotificacoesController> logger)
    {
        _historicoRepository = historicoRepository;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lista os logs de notificações com filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HistoricoNotificacaoResponse>>> Listar(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] StatusNotificacao? status,
        [FromQuery] string? emailDestinatario,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaClienteId = Guid.Parse(User.FindFirstValue("EmpresaClienteId")!);

            var historicos = await _historicoRepository.GetByFiltrosAsync(
                empresaClienteId,
                dataInicio,
                dataFim,
                status,
                emailDestinatario,
                cancellationToken);

            // Buscar nomes dos usuários em uma única query
            var usuariosIds = historicos
                .Where(h => h.UsuarioCriacaoId.HasValue)
                .Select(h => h.UsuarioCriacaoId!.Value)
                .Distinct()
                .ToList();

            var usuarios = await _usuarioRepository.GetByIdsAsync(usuariosIds, cancellationToken);
            var usuariosDict = usuarios.ToDictionary(u => u.Id, u => u.Nome);

            var response = historicos.Select(h =>
            {
                var (email, telefone) = ExtrairDestinatario(h.Cobranca?.PayloadJson);

                return new HistoricoNotificacaoResponse
                {
                    Id = h.Id,
                    CobrancaId = h.CobrancaId,
                    RegraCobrancaId = h.RegraCobrancaId,
                    NomeRegra = h.RegraCobranca?.Nome ?? "N/A",
                    CanalUtilizado = h.CanalUtilizado,
                    Status = h.Status,
                    StatusTexto = ObterTextoStatus(h.Status),
                    EmailDestinatario = email,
                    TelefoneDestinatario = telefone,
                    Assunto = ExtrairAssunto(h.MensagemEnviada),
                    DataEnvio = h.DataEnvio,
                    MensagemErro = h.MensagemErro,
                    MotivoRejeicao = h.MotivoRejeicao,
                    QuantidadeAberturas = h.QuantidadeAberturas,
                    DataPrimeiraAbertura = h.DataPrimeiraAbertura,
                    DataUltimaAbertura = h.DataUltimaAbertura,
                    QuantidadeCliques = h.QuantidadeCliques,
                    DataPrimeiroClique = h.DataPrimeiroClique,
                    DataUltimoClique = h.DataUltimoClique,
                    LinkClicado = h.LinkClicado,
                    MessageIdProvedor = h.MessageIdProvedor,
                    UsuarioCriacaoId = h.UsuarioCriacaoId,
                    NomeUsuarioCriacao = h.UsuarioCriacaoId.HasValue && usuariosDict.ContainsKey(h.UsuarioCriacaoId.Value)
                        ? usuariosDict[h.UsuarioCriacaoId.Value]
                        : null
                };
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar notificações");
            return StatusCode(500, new { message = "Erro ao listar notificações" });
        }
    }

    /// <summary>
    /// Obtém detalhes de uma notificação específica
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<HistoricoNotificacaoResponse>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var empresaClienteId = Guid.Parse(User.FindFirstValue("EmpresaClienteId")!);

            var historico = await _historicoRepository.GetByIdAsync(id, cancellationToken);

            if (historico == null || historico.EmpresaClienteId != empresaClienteId)
            {
                return NotFound(new { message = "Notificação não encontrada" });
            }

            var (email, telefone) = ExtrairDestinatario(historico.Cobranca?.PayloadJson);

            var response = new HistoricoNotificacaoResponse
            {
                Id = historico.Id,
                CobrancaId = historico.CobrancaId,
                RegraCobrancaId = historico.RegraCobrancaId,
                NomeRegra = historico.RegraCobranca?.Nome ?? "N/A",
                CanalUtilizado = historico.CanalUtilizado,
                Status = historico.Status,
                StatusTexto = ObterTextoStatus(historico.Status),
                EmailDestinatario = email,
                TelefoneDestinatario = telefone,
                Assunto = ExtrairAssunto(historico.MensagemEnviada),
                DataEnvio = historico.DataEnvio,
                MensagemErro = historico.MensagemErro,
                MotivoRejeicao = historico.MotivoRejeicao,
                QuantidadeAberturas = historico.QuantidadeAberturas,
                DataPrimeiraAbertura = historico.DataPrimeiraAbertura,
                DataUltimaAbertura = historico.DataUltimaAbertura,
                QuantidadeCliques = historico.QuantidadeCliques,
                DataPrimeiroClique = historico.DataPrimeiroClique,
                DataUltimoClique = historico.DataUltimoClique,
                LinkClicado = historico.LinkClicado,
                MessageIdProvedor = historico.MessageIdProvedor
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificação {Id}", id);
            return StatusCode(500, new { message = "Erro ao obter notificação" });
        }
    }

    private string ObterTextoStatus(StatusNotificacao status)
    {
        return status switch
        {
            StatusNotificacao.Pendente => "Pendente",
            StatusNotificacao.Enviado => "Enviado",
            StatusNotificacao.Entregue => "Entregue",
            StatusNotificacao.Aberto => "Aberto",
            StatusNotificacao.Clicado => "Clicado",
            StatusNotificacao.SoftBounce => "Erro Temporário",
            StatusNotificacao.Adiado => "Adiado",
            StatusNotificacao.HardBounce => "Erro Permanente",
            StatusNotificacao.EmailInvalido => "Email Inválido",
            StatusNotificacao.Bloqueado => "Bloqueado",
            StatusNotificacao.Reclamacao => "Marcado como Spam",
            StatusNotificacao.Descadastrado => "Descadastrado",
            StatusNotificacao.ErroEnvio => "Erro no Envio",
            _ => status.ToString()
        };
    }

    private (string? email, string? telefone) ExtrairDestinatario(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            return (null, null);

        try
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(payloadJson);
            if (payload == null)
                return (null, null);

            string? email = null;
            string? telefone = null;

            // Tentar extrair email
            if (payload.TryGetValue("email", out var emailElement))
                email = emailElement.GetString();
            else if (payload.TryGetValue("Email", out emailElement))
                email = emailElement.GetString();

            // Tentar extrair telefone
            if (payload.TryGetValue("telefone", out var telefoneElement))
                telefone = telefoneElement.GetString();
            else if (payload.TryGetValue("Telefone", out telefoneElement))
                telefone = telefoneElement.GetString();

            return (email, telefone);
        }
        catch
        {
            return (null, null);
        }
    }

    private string? ExtrairAssunto(string mensagem)
    {
        // Tentar extrair o assunto da mensagem HTML
        // Isso é uma implementação simplificada - pode ser melhorada
        if (string.IsNullOrWhiteSpace(mensagem))
            return null;

        // Se a mensagem começar com "Subject:", extrair
        if (mensagem.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
        {
            var linhas = mensagem.Split('\n');
            if (linhas.Length > 0)
            {
                return linhas[0].Replace("Subject:", "").Trim();
            }
        }

        // Caso contrário, retornar os primeiros 100 caracteres
        return mensagem.Length > 100 ? mensagem.Substring(0, 100) + "..." : mensagem;
    }
}
