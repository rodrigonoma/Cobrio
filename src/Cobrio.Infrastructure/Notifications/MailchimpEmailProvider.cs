using Cobrio.Application.Interfaces;
using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Cobrio.Infrastructure.Notifications;

public class BrevoEmailProvider : IEmailProvider
{
    private readonly IEmailService _emailService;
    private readonly ILogger<BrevoEmailProvider> _logger;

    public BrevoEmailProvider(
        IEmailService emailService,
        ILogger<BrevoEmailProvider> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public CanalNotificacao TipoCanal => CanalNotificacao.Email;

    public string ProviderName => "Brevo (Sendinblue)";

    public async Task<NotificationResult> EnviarAsync(
        string destinatario,
        string mensagem,
        string? assunto = null,
        CancellationToken cancellationToken = default)
    {
        return await EnviarEmailAsync(
            destinatario,
            assunto ?? "Notificação Cobrio",
            mensagem,
            true,
            null,
            cancellationToken);
    }

    public async Task<NotificationResult> EnviarEmailAsync(
        string destinatario,
        string assunto,
        string corpo,
        bool isHtml = true,
        string? remetenteNome = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Enviando email via {Provider} para {Destinatario}",
                ProviderName,
                destinatario);

            var sucesso = await _emailService.EnviarEmailAsync(
                destinatario,
                assunto,
                corpo,
                cancellationToken);

            if (sucesso)
            {
                return NotificationResult.ComSucesso(
                    $"Email enviado via {ProviderName}",
                    Guid.NewGuid().ToString());
            }
            else
            {
                return NotificationResult.ComFalha(
                    "Falha ao enviar email via Brevo",
                    "Email não foi aceito pelo provider");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email via {Provider}", ProviderName);
            return NotificationResult.ComFalha(
                $"Erro ao enviar email: {ex.Message}",
                ex.ToString());
        }
    }
}
