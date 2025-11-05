using Cobrio.Domain.Enums;

namespace Cobrio.Application.Interfaces.Notifications;

/// <summary>
/// Factory para criar instâncias de canais de notificação
/// </summary>
public interface INotificationChannelFactory
{
    /// <summary>
    /// Obtém o provider configurado para um tipo de canal
    /// </summary>
    INotificationChannel ObterCanal(CanalNotificacao tipoCanal);
}
