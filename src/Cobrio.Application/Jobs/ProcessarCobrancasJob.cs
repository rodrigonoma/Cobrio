using System.Text.Json;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cobrio.Application.Jobs;

public class ProcessarCobrancasJob
{
    private readonly ICobrancaRepository _cobrancaRepository;
    private readonly IHistoricoNotificacaoRepository _historicoRepository;
    private readonly IRegraCobrancaRepository _regraRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessarCobrancasJob> _logger;

    public ProcessarCobrancasJob(
        ICobrancaRepository cobrancaRepository,
        IHistoricoNotificacaoRepository historicoRepository,
        IRegraCobrancaRepository regraRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<ProcessarCobrancasJob> logger)
    {
        _cobrancaRepository = cobrancaRepository;
        _historicoRepository = historicoRepository;
        _regraRepository = regraRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecutarAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processamento de cobranças pendentes...");

        try
        {
            // Buscar cobranças pendentes para processar
            var cobrancasPendentes = await _cobrancaRepository.GetPendentesParaProcessarAsync(cancellationToken);

            _logger.LogInformation("Encontradas {Count} cobranças para processar", cobrancasPendentes.Count());

            foreach (var cobranca in cobrancasPendentes)
            {
                try
                {
                    await ProcessarCobrancaAsync(cobranca, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar cobrança {CobrancaId}", cobranca.Id);
                }
            }

            _logger.LogInformation("Processamento de cobranças concluído");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar job de processamento de cobranças");
            throw;
        }
    }

    private async Task ProcessarCobrancaAsync(Cobranca cobranca, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando cobrança {CobrancaId}", cobranca.Id);

        // Buscar regra de cobrança
        var regra = await _regraRepository.GetByIdAsync(cobranca.RegraCobrancaId, cancellationToken);

        if (regra == null || !regra.Ativa)
        {
            _logger.LogWarning("Regra de cobrança {RegraId} não encontrada ou inativa", cobranca.RegraCobrancaId);
            cobranca.MarcarComoFalha("Regra de cobrança não encontrada ou inativa");
            _cobrancaRepository.Update(cobranca);
            await _unitOfWork.CommitAsync(cancellationToken);
            return;
        }

        try
        {
            // Deserializar payload
            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(cobranca.PayloadJson);

            if (payload == null)
            {
                throw new InvalidOperationException("Payload inválido");
            }

            // Processar template
            var mensagem = regra.ProcessarTemplate(payload);

            // Extrair destinatário do payload
            var destinatario = ExtrairDestinatario(regra.CanalNotificacao, payload);

            if (string.IsNullOrEmpty(destinatario))
            {
                throw new InvalidOperationException(
                    $"Campo destinatário não encontrado no payload para canal {regra.CanalNotificacao}");
            }

            // Enviar notificação usando provider real
            var resultado = await _notificationService.EnviarAsync(
                regra.CanalNotificacao,
                destinatario,
                mensagem,
                "Cobrança Cobrio", // assunto para email
                cancellationToken);

            if (resultado.Sucesso)
            {
                cobranca.MarcarComoProcessada();

                // Registrar histórico de sucesso
                var historico = HistoricoNotificacao.CriarSucesso(
                    cobranca.Id,
                    cobranca.RegraCobrancaId,
                    cobranca.EmpresaClienteId,
                    regra.CanalNotificacao,
                    mensagem,
                    cobranca.PayloadJson,
                    resultado.RespostaProvedor ?? "Enviado com sucesso"
                );

                await _historicoRepository.AddAsync(historico, cancellationToken);
                _logger.LogInformation(
                    "Cobrança {CobrancaId} processada com sucesso. IdRastreamento: {IdRastreamento}",
                    cobranca.Id,
                    resultado.IdRastreamento);
            }
            else
            {
                throw new Exception(resultado.MensagemErro ?? "Falha no envio");
            }

            _cobrancaRepository.Update(cobranca);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar cobrança {CobrancaId}", cobranca.Id);

            cobranca.MarcarComoFalha($"Erro: {ex.Message}");

            // Registrar histórico de falha
            var historico = HistoricoNotificacao.CriarFalha(
                cobranca.Id,
                cobranca.RegraCobrancaId,
                cobranca.EmpresaClienteId,
                regra.CanalNotificacao,
                string.Empty,
                cobranca.PayloadJson,
                ex.Message
            );

            await _historicoRepository.AddAsync(historico, cancellationToken);
            _cobrancaRepository.Update(cobranca);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Se ainda pode tentar novamente, não lança exceção
            if (cobranca.PodeSerProcessada())
            {
                _logger.LogWarning("Cobrança {CobrancaId} será reprocessada. Tentativa {Tentativa}/5",
                    cobranca.Id, cobranca.TentativasEnvio);
            }
        }
    }

    /// <summary>
    /// Extrai o destinatário do payload baseado no tipo de canal
    /// Usa convenção: "Email" para email, "Telefone" ou "Celular" para SMS/WhatsApp
    /// </summary>
    private string? ExtrairDestinatario(CanalNotificacao canal, Dictionary<string, object> payload)
    {
        return canal switch
        {
            CanalNotificacao.Email => ObterValor(payload, "Email", "email", "EmailDestinatario"),
            CanalNotificacao.SMS => ObterValor(payload, "Telefone", "Celular", "telefone", "celular", "Numero"),
            CanalNotificacao.WhatsApp => ObterValor(payload, "Telefone", "Celular", "WhatsApp", "telefone", "celular", "Numero"),
            _ => null
        };
    }

    /// <summary>
    /// Tenta obter valor de múltiplas chaves possíveis (case-insensitive)
    /// </summary>
    private string? ObterValor(Dictionary<string, object> payload, params string[] chavesPossiveis)
    {
        foreach (var chave in chavesPossiveis)
        {
            var encontrado = payload.FirstOrDefault(kvp =>
                kvp.Key.Equals(chave, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(encontrado.Key))
            {
                return encontrado.Value?.ToString();
            }
        }

        return null;
    }
}
