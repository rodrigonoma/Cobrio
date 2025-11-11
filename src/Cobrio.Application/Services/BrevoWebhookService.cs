using Cobrio.Application.DTOs.Brevo;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cobrio.Application.Services;

public class BrevoWebhookService
{
    private readonly IHistoricoNotificacaoRepository _historicoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BrevoWebhookService> _logger;

    public BrevoWebhookService(
        IHistoricoNotificacaoRepository historicoRepository,
        IUnitOfWork unitOfWork,
        ILogger<BrevoWebhookService> logger)
    {
        _historicoRepository = historicoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ProcessarEventoAsync(BrevoWebhookEvent evento, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processando evento Brevo: {Event} para {Email} (MessageId: {MessageId})",
                evento.Event, evento.Email, evento.MessageId);

            // Buscar o histórico pelo MessageId do provedor
            var historico = await _historicoRepository.GetByMessageIdProvedor(evento.MessageId ?? evento.Id.ToString(), cancellationToken);

            if (historico == null)
            {
                _logger.LogWarning("Histórico não encontrado para MessageId: {MessageId}. Email: {Email}",
                    evento.MessageId ?? evento.Id.ToString(), evento.Email);
                return false;
            }

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
                    var dataAbertura = ConverterTimestamp(evento.TsEvent ?? evento.Ts);
                    historico.RegistrarAbertura(dataAbertura, evento.Ip, evento.UserAgent);
                    break;

                case "clicked":
                case "click":
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
                    return false;
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Evento processado com sucesso: {Event} para {Email}", evento.Event, evento.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento Brevo: {Event} para {Email}", evento.Event, evento.Email);
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
