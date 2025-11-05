using Cobrio.Application.Interfaces;
using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Cobrio.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationChannelFactory _channelFactory;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationChannelFactory channelFactory,
        ILogger<NotificationService> logger)
    {
        _channelFactory = channelFactory;
        _logger = logger;
    }

    public async Task<NotificationResult> EnviarAsync(
        CanalNotificacao canal,
        string destinatario,
        string mensagem,
        string? assunto = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Enviando notificação via {Canal} para {Destinatario}",
                canal,
                destinatario);

            var provider = _channelFactory.ObterCanal(canal);

            var resultado = await provider.EnviarAsync(
                destinatario,
                mensagem,
                assunto,
                cancellationToken);

            if (resultado.Sucesso)
            {
                _logger.LogInformation(
                    "Notificação enviada com sucesso. Canal: {Canal}, IdRastreamento: {Id}",
                    canal,
                    resultado.IdRastreamento);
            }
            else
            {
                _logger.LogWarning(
                    "Falha ao enviar notificação. Canal: {Canal}, Erro: {Erro}",
                    canal,
                    resultado.MensagemErro);
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação via {Canal}", canal);

            return NotificationResult.ComFalha(
                $"Erro inesperado: {ex.Message}",
                ex.ToString());
        }
    }
}
