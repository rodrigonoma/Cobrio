using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;

namespace Cobrio.Infrastructure.Repositories;

public class BrevoWebhookLogRepository : IBrevoWebhookLogRepository
{
    private readonly CobrioDbContext _context;

    public BrevoWebhookLogRepository(CobrioDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BrevoWebhookLog log, CancellationToken cancellationToken = default)
    {
        await _context.BrevoWebhookLogs.AddAsync(log, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
