namespace Cobrio.Application.Interfaces.Notifications;

/// <summary>
/// Interface específica para providers de WhatsApp
/// </summary>
public interface IWhatsAppProvider : INotificationChannel
{
    /// <summary>
    /// Nome do provider (ex: "Twilio", "360Dialog", "MessageBird")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Envia mensagem via WhatsApp
    /// </summary>
    Task<NotificationResult> EnviarWhatsAppAsync(
        string numeroDestino,
        string mensagem,
        CancellationToken cancellationToken = default);
}
