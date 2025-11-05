namespace Cobrio.Application.Interfaces.Notifications;

/// <summary>
/// Interface específica para providers de SMS
/// </summary>
public interface ISmsProvider : INotificationChannel
{
    /// <summary>
    /// Nome do provider (ex: "Twilio", "Vonage", "AmazonSNS")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Envia SMS
    /// </summary>
    Task<NotificationResult> EnviarSmsAsync(
        string numeroDestino,
        string mensagem,
        CancellationToken cancellationToken = default);
}
