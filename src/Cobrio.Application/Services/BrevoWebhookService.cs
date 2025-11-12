using Cobrio.Application.DTOs.Brevo;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Cobrio.Application.Services;

public class BrevoWebhookService
{
    private readonly IHistoricoNotificacaoRepository _historicoRepository;
    private readonly IBrevoWebhookLogRepository _webhookLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BrevoWebhookService> _logger;

    public BrevoWebhookService(
        IHistoricoNotificacaoRepository historicoRepository,
        IBrevoWebhookLogRepository webhookLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<BrevoWebhookService> logger)
    {
        _historicoRepository = historicoRepository;
        _webhookLogRepository = webhookLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ProcessarEventoAsync(
        BrevoWebhookEvent evento,
        string? enderecoIp = null,
        string? userAgent = null,
        string? headers = null,
        CancellationToken cancellationToken = default)
    {
        BrevoWebhookLog? webhookLog = null;

        try
        {
            _logger.LogInformation(
                "🔔 Webhook Brevo recebido - Evento: {Event} | Email: {Email} | MessageId: {MessageId} | Id: {Id}",
                evento.Event, evento.Email, evento.MessageId, evento.Id);

            // 1. SEMPRE salvar o log do webhook PRIMEIRO (para auditoria e debug)
            var payloadJson = JsonSerializer.Serialize(evento, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var dataEvento = ConverterTimestamp(evento.TsEvent ?? evento.Ts);

            webhookLog = new BrevoWebhookLog(
                evento.Event,
                evento.Email,
                evento.MessageId,
                evento.Id,
                payloadJson,
                dataEvento,
                enderecoIp,
                userAgent,
                headers);

            await _webhookLogRepository.AddAsync(webhookLog, cancellationToken);
            await _webhookLogRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Webhook log salvo - LogId: {LogId}", webhookLog.Id);

            // 2. Processar o evento - Buscar histórico com múltiplas estratégias
            HistoricoNotificacao? historico = null;
            var tentativasBusca = new List<string>();

            _logger.LogInformation(
                "🎯 INICIANDO BUSCA DO HISTÓRICO | MessageId: '{MessageId}' | Id: {Id} | Email: {Email}",
                evento.MessageId ?? "NULL", evento.Id, evento.Email);

            // Estratégia 1: Buscar pelo Message-ID (RFC 2822)
            if (!string.IsNullOrWhiteSpace(evento.MessageId))
            {
                _logger.LogInformation("🔍 [1/3] Buscando histórico pelo Message-ID RFC 2822: '{MessageId}'", evento.MessageId);
                historico = await _historicoRepository.GetByMessageIdProvedor(evento.MessageId, cancellationToken);
                tentativasBusca.Add($"Message-ID '{evento.MessageId}'");

                if (historico != null)
                {
                    _logger.LogInformation("✅ [1/3] ENCONTRADO pelo Message-ID! HistoricoId: {Id}", historico.Id);
                }
                else
                {
                    _logger.LogWarning("❌ [1/3] NÃO encontrado pelo Message-ID");
                }
            }

            // Estratégia 2: Buscar pelo ID numérico do Brevo
            if (historico == null && evento.Id > 0)
            {
                var idNumerico = evento.Id.ToString();
                _logger.LogInformation("🔍 [2/3] Buscando histórico pelo ID numérico Brevo: '{Id}'", idNumerico);
                historico = await _historicoRepository.GetByMessageIdProvedor(idNumerico, cancellationToken);
                tentativasBusca.Add($"ID numérico '{idNumerico}'");

                if (historico != null)
                {
                    _logger.LogInformation("✅ [2/3] ENCONTRADO pelo ID numérico! HistoricoId: {Id}", historico.Id);
                }
                else
                {
                    _logger.LogWarning("❌ [2/3] NÃO encontrado pelo ID numérico");
                }
            }

            // Estratégia 3: Buscar por email + data aproximada (fallback)
            if (historico == null && !string.IsNullOrWhiteSpace(evento.Email))
            {
                var dataEventoBusca = ConverterTimestamp(evento.TsEvent ?? evento.Ts);
                _logger.LogInformation(
                    "🔍 [3/3] Buscando histórico por Email + Data (fallback): '{Email}' perto de {Data} (tolerância: ±60 min)",
                    evento.Email, dataEventoBusca);
                historico = await _historicoRepository.GetByEmailEDataAsync(evento.Email, dataEventoBusca, toleranciaMinutos: 60, cancellationToken);
                tentativasBusca.Add($"Email '{evento.Email}' + Data '{dataEventoBusca:yyyy-MM-dd HH:mm}'");

                if (historico != null)
                {
                    _logger.LogInformation("✅ [3/3] ENCONTRADO por Email+Data! HistoricoId: {Id} | MessageIdProvedor: '{MessageId}' | DataEnvio: {DataEnvio}",
                        historico.Id, historico.MessageIdProvedor ?? "NULL", historico.DataEnvio);
                }
                else
                {
                    _logger.LogWarning("❌ [3/3] NÃO encontrado por Email+Data");
                }
            }

            if (historico == null)
            {
                var tentativas = string.Join(", ", tentativasBusca);
                _logger.LogError(
                    "🚨 HISTÓRICO NÃO ENCONTRADO APÓS {Tentativas} TENTATIVAS | Email: {Email} | Evento: {Event} | Tentativas: [{TentativasDetalhes}]",
                    tentativasBusca.Count, evento.Email, evento.Event, tentativas);

                _logger.LogError(
                    "🚨 DADOS DO EVENTO: MessageId='{MessageId}' | Id={Id} | TsEvent={TsEvent} | Date='{Date}'",
                    evento.MessageId ?? "NULL", evento.Id, evento.TsEvent, evento.Date ?? "NULL");

                webhookLog.MarcarComoFalha($"Histórico não encontrado. Tentativas: {tentativas}");
                await _webhookLogRepository.SaveChangesAsync(cancellationToken);

                return false;
            }

            _logger.LogInformation(
                "✅ Histórico ENCONTRADO - Id: {HistoricoId} | CobrancaId: {CobrancaId} | Status atual: {Status}",
                historico.Id, historico.CobrancaId, historico.Status);

            // Processar o evento baseado no tipo
            switch (evento.Event.ToLowerInvariant())
            {
                case "request":
                case "sent":
                    historico.AtualizarStatus(StatusNotificacao.Enviado);
                    historico.RegistrarMessageId(evento.MessageId ?? evento.Id.ToString());
                    break;

                case "delivered":
                    historico.AtualizarStatus(StatusNotificacao.Entregue);
                    break;

                case "opened":
                case "open":
                case "unique_opened":
                    var dataAbertura = ConverterTimestamp(evento.TsEvent ?? evento.Ts);
                    _logger.LogInformation(
                        "📧 Registrando ABERTURA - Data: {Data} | IP: {Ip} | UserAgent: {UserAgent}",
                        dataAbertura, evento.Ip, evento.UserAgent);
                    historico.RegistrarAbertura(dataAbertura, evento.Ip, evento.UserAgent);
                    _logger.LogInformation(
                        "✅ Abertura registrada - Qtd aberturas: {Qtd} | Novo status: {Status}",
                        historico.QuantidadeAberturas, historico.Status);
                    break;

                case "clicked":
                case "click":
                case "unique_clicked":
                    var dataClique = ConverterTimestamp(evento.TsEvent ?? evento.Ts);
                    historico.RegistrarClique(dataClique, evento.Link);
                    break;

                case "soft_bounce":
                case "soft-bounce":
                    historico.AtualizarStatus(StatusNotificacao.SoftBounce, evento.Reason, evento.Code);
                    break;

                case "hard_bounce":
                case "hard-bounce":
                    historico.AtualizarStatus(StatusNotificacao.HardBounce, evento.Reason, evento.Code);
                    break;

                case "invalid_email":
                case "invalid-email":
                    historico.AtualizarStatus(StatusNotificacao.EmailInvalido, evento.Reason, evento.Code);
                    break;

                case "deferred":
                    historico.AtualizarStatus(StatusNotificacao.Adiado, evento.Reason, evento.Code);
                    break;

                case "blocked":
                    historico.AtualizarStatus(StatusNotificacao.Bloqueado, evento.Reason, evento.Code);
                    break;

                case "complaint":
                case "spam":
                    historico.AtualizarStatus(StatusNotificacao.Reclamacao, evento.Reason);
                    break;

                case "unsubscribed":
                case "unsubscribe":
                    historico.AtualizarStatus(StatusNotificacao.Descadastrado);
                    break;

                case "error":
                    historico.AtualizarStatus(StatusNotificacao.ErroEnvio, evento.Reason, evento.Code);
                    break;

                default:
                    _logger.LogWarning("Evento desconhecido do Brevo: {Event}", evento.Event);
                    if (webhookLog != null)
                    {
                        webhookLog.MarcarComoFalha($"Tipo de evento desconhecido: {evento.Event}");
                        await _webhookLogRepository.SaveChangesAsync(cancellationToken);
                    }
                    return false;
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            // Marcar webhook log como processado com sucesso
            if (webhookLog != null)
            {
                webhookLog.MarcarComoProcessado(historico.Id);
                await _webhookLogRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("✅ Evento processado com sucesso: {Event} para {Email}", evento.Event, evento.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar evento Brevo: {Event} para {Email}", evento.Event, evento.Email);

            // Marcar webhook log como falha
            if (webhookLog != null)
            {
                try
                {
                    webhookLog.MarcarComoFalha($"Exceção: {ex.Message}");
                    await _webhookLogRepository.SaveChangesAsync(cancellationToken);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Erro ao salvar falha do webhook log");
                }
            }

            return false;
        }
    }

    private DateTime ConverterTimestamp(long? timestamp)
    {
        if (timestamp == null)
            return DateTime.UtcNow;

        return DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).UtcDateTime;
    }
}
