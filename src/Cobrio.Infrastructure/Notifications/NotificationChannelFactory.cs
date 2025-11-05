using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Cobrio.Infrastructure.Notifications;

/// <summary>
/// Factory que resolve o provider correto baseado no tipo de canal
/// Permite fácil extensão para adicionar novos providers
/// </summary>
public class NotificationChannelFactory : INotificationChannelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationChannelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public INotificationChannel ObterCanal(CanalNotificacao tipoCanal)
    {
        return tipoCanal switch
        {
            CanalNotificacao.Email => _serviceProvider.GetRequiredService<IEmailProvider>(),
            CanalNotificacao.SMS => _serviceProvider.GetRequiredService<ISmsProvider>(),
            CanalNotificacao.WhatsApp => _serviceProvider.GetRequiredService<IWhatsAppProvider>(),
            _ => throw new NotSupportedException($"Canal de notificação {tipoCanal} não suportado")
        };
    }
}
