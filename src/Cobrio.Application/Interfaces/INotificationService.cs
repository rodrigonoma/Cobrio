using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;

namespace Cobrio.Application.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Envia notificação usando o canal especificado
    /// </summary>
    Task<NotificationResult> EnviarAsync(
        CanalNotificacao canal,
        string destinatario,
        string mensagem,
        string? assunto = null,
        CancellationToken cancellationToken = default);
}
