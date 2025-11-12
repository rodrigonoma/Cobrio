using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface IBrevoWebhookLogRepository
{
    Task AddAsync(BrevoWebhookLog log, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
