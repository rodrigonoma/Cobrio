namespace Cobrio.Application.Interfaces.Notifications;

/// <summary>
/// Interface específica para providers de email
/// </summary>
public interface IEmailProvider : INotificationChannel
{
    /// <summary>
    /// Nome do provider (ex: "SendGrid", "Mailgun", "AmazonSES")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Envia email com suporte a anexos e HTML
    /// </summary>
    Task<NotificationResult> EnviarEmailAsync(
        string destinatario,
        string assunto,
        string corpo,
        bool isHtml = true,
        string? remetenteEmail = null,
        string? remetenteNome = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default);
}
