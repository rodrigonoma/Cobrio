using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Cobrio.Infrastructure.Notifications;

public class SendGridEmailProvider : IEmailProvider
{
    private readonly SendGridClient _client;
    private readonly SendGridSettings _settings;
    private readonly ILogger<SendGridEmailProvider> _logger;

    public CanalNotificacao TipoCanal => CanalNotificacao.Email;
    public string ProviderName => "SendGrid";

    public SendGridEmailProvider(
        IOptions<SendGridSettings> settings,
        ILogger<SendGridEmailProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new SendGridClient(_settings.ApiKey);
    }

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
            isHtml: true,
            cancellationToken: cancellationToken);
    }

    public async Task<NotificationResult> EnviarEmailAsync(
        string destinatario,
        string assunto,
        string corpo,
        bool isHtml = true,
        string? remetenteEmail = null,
        string? remetenteNome = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Usar parâmetros se fornecidos, senão usar configuração padrão
            var fromEmail = remetenteEmail ?? _settings.RemetenteEmail;
            var fromName = remetenteNome ?? _settings.RemetenteNome;

            _logger.LogInformation(
                "Enviando email via SendGrid para {Destinatario} de {FromName} <{FromEmail}>",
                destinatario, fromName, fromEmail);

            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(destinatario);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                assunto,
                isHtml ? null : corpo,
                isHtml ? corpo : null);

            // Adicionar Reply-To se fornecido
            if (!string.IsNullOrWhiteSpace(replyTo))
            {
                msg.ReplyTo = new EmailAddress(replyTo);
                _logger.LogInformation("Reply-To configurado para: {ReplyTo}", replyTo);
            }

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault();

                _logger.LogInformation(
                    "Email enviado com sucesso via SendGrid. MessageId: {MessageId}",
                    messageId);

                return NotificationResult.ComSucesso(
                    $"Status: {response.StatusCode}",
                    messageId);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);

                _logger.LogWarning(
                    "Falha ao enviar email via SendGrid. Status: {StatusCode}, Body: {Body}",
                    response.StatusCode,
                    body);

                return NotificationResult.ComFalha(
                    $"Falha no envio: {response.StatusCode}",
                    body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email via SendGrid para {Destinatario}", destinatario);

            return NotificationResult.ComFalha(
                $"Exceção: {ex.Message}",
                ex.ToString());
        }
    }
}

public class SendGridSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string RemetenteEmail { get; set; } = string.Empty;
    public string RemetenteNome { get; set; } = string.Empty;
}
