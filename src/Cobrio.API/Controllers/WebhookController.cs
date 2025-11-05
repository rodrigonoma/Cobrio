using Cobrio.Application.DTOs.Cobranca;
using Cobrio.Application.Services;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Endpoint público sem autenticação
public class WebhookController : ControllerBase
{
    private readonly CobrancaService _cobrancaService;
    private readonly IRegraCobrancaRepository _regraRepository;
    private readonly IHistoricoImportacaoRepository _historicoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        CobrancaService cobrancaService,
        IRegraCobrancaRepository regraRepository,
        IHistoricoImportacaoRepository historicoRepository,
        IUnitOfWork unitOfWork,
        ILogger<WebhookController> logger)
    {
        _cobrancaService = cobrancaService;
        _regraRepository = regraRepository;
        _historicoRepository = historicoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint público para receber cobranças via webhook (aceita array ou objeto único)
    /// </summary>
    /// <param name="token">Token único da regra de cobrança</param>
    /// <param name="requests">Lista de cobranças ou objeto único com campos obrigatórios e variáveis customizadas</param>
    /// <returns>Cobranças criadas</returns>
    /// <remarks>
    /// Exemplo de requisição (array):
    ///
    ///     POST /api/webhook/{token}
    ///     [
    ///       {
    ///         "nomeCliente": "João Silva",
    ///         "email": "joao@example.com",
    ///         "telefone": "+5511999999999",
    ///         "payload": {
    ///             "Valor": "150.00",
    ///             "DataVencimento": "15/11/2025",
    ///             "LinkPagamento": "https://seusite.com/pagar/123"
    ///         },
    ///         "dataVencimento": "2025-11-15T23:59:59"
    ///       }
    ///     ]
    ///
    /// Campos obrigatórios (definidos na regra):
    /// - dataVencimento: sempre obrigatório
    /// - Outros campos: conforme configurado na regra (ex: email, telefone, nomeCliente)
    /// - payload: variáveis definidas no template da regra
    /// </remarks>
    [HttpPost("{token}")]
    public async Task<IActionResult> ReceberCobranca(
        string token,
        [FromBody] List<CreateCobrancaRequest> requests)
    {
        RegraCobranca? regra = null;
        var totalLinhas = 0;
        var linhasProcessadas = 0;
        var linhasComErro = 0;
        var erros = new List<object>();

        try
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token é obrigatório" });

            if (requests == null || !requests.Any())
                return BadRequest(new { message = "Lista de cobranças é obrigatória" });

            // Buscar regra pelo token para ter o ID para o histórico
            regra = await _regraRepository.GetByTokenAsync(token);
            if (regra == null)
                throw new UnauthorizedAccessException("Token inválido");

            totalLinhas = requests.Count;

            _logger.LogInformation("Recebendo {Count} cobrança(s) via webhook. Token: {Token}, Regra: {RegraId}",
                totalLinhas, token, regra.Id);

            var cobrancasCriadas = new List<object>();

            for (int i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                var numeroLinha = i + 1;

                try
                {
                    _logger.LogInformation("Processando cobrança {Linha}/{Total} para cliente: {NomeCliente}",
                        numeroLinha, totalLinhas, request.NomeCliente);

                    var cobranca = await _cobrancaService.CreateByTokenAsync(token, request);

                    _logger.LogInformation("Cobrança criada com sucesso. ID: {CobrancaId}, DataDisparo: {DataDisparo}",
                        cobranca.Id, cobranca.DataDisparo);

                    cobrancasCriadas.Add(new
                    {
                        cobrancaId = cobranca.Id,
                        dataDisparo = cobranca.DataDisparo,
                        status = cobranca.Status
                    });

                    linhasProcessadas++;
                }
                catch (Exception ex)
                {
                    linhasComErro++;
                    var erro = new
                    {
                        NumeroLinha = numeroLinha,
                        TipoErro = ex.GetType().Name,
                        Descricao = ex.Message,
                        ValorInvalido = request.NomeCliente ?? "N/A"
                    };
                    erros.Add(erro);

                    _logger.LogWarning(ex, "Erro ao processar cobrança na linha {Linha}: {Mensagem}",
                        numeroLinha, ex.Message);
                }
            }

            // Salvar histórico da importação via webhook
            await SalvarHistoricoWebhook(regra, totalLinhas, linhasProcessadas, linhasComErro, erros);

            var statusCode = linhasComErro > 0 && linhasProcessadas == 0 ? 400 : 200;

            return StatusCode(statusCode, new
            {
                message = $"{linhasProcessadas} de {totalLinhas} cobrança(s) processada(s) com sucesso",
                total = totalLinhas,
                processadas = linhasProcessadas,
                comErro = linhasComErro,
                cobrancas = cobrancasCriadas,
                erros = linhasComErro > 0 ? erros : null
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token inválido: {Token}", token);
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Operação inválida: {Message}", ex.Message);

            // Salvar histórico de erro se temos a regra
            if (regra != null)
                await SalvarHistoricoWebhook(regra, totalLinhas, linhasProcessadas, linhasComErro, erros);

            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Dados inválidos: {Message}", ex.Message);

            // Salvar histórico de erro se temos a regra
            if (regra != null)
                await SalvarHistoricoWebhook(regra, totalLinhas, linhasProcessadas, linhasComErro, erros);

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar cobranças via webhook");

            // Salvar histórico de erro se temos a regra
            if (regra != null)
                await SalvarHistoricoWebhook(regra, totalLinhas, linhasProcessadas, linhasComErro, erros);

            return StatusCode(500, new { message = "Erro interno ao processar cobranças" });
        }
    }

    private async Task SalvarHistoricoWebhook(
        RegraCobranca regra,
        int totalLinhas,
        int linhasProcessadas,
        int linhasComErro,
        List<object> erros)
    {
        try
        {
            var statusImportacao = linhasProcessadas == totalLinhas
                ? StatusImportacao.Sucesso
                : linhasProcessadas > 0
                    ? StatusImportacao.Parcial
                    : StatusImportacao.Erro;

            var errosJson = erros.Any()
                ? JsonSerializer.Serialize(erros)
                : null;

            var historico = new HistoricoImportacao(
                regra.Id,
                regra.EmpresaClienteId,
                null, // UsuarioId - webhook não tem usuário autenticado
                $"webhook-{DateTime.Now:yyyyMMdd-HHmmss}.json", // Nome fictício para identificar origem webhook
                totalLinhas,
                linhasProcessadas,
                linhasComErro,
                statusImportacao,
                OrigemImportacao.Webhook,
                errosJson);

            await _historicoRepository.AddAsync(historico);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Histórico de webhook salvo. Regra: {RegraId}, Total: {Total}, Processadas: {Processadas}, Erros: {Erros}",
                regra.Id, totalLinhas, linhasProcessadas, linhasComErro);
        }
        catch (Exception ex)
        {
            // Não propagar erro do histórico para não afetar a resposta principal
            _logger.LogError(ex, "Erro ao salvar histórico de webhook. Regra: {RegraId}", regra.Id);
        }
    }

    /// <summary>
    /// Endpoint para testar se o webhook está funcionando
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "online", timestamp = DateTime.UtcNow });
    }
}
