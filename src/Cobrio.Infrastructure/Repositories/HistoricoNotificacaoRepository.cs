using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class HistoricoNotificacaoRepository : Repository<HistoricoNotificacao>, IHistoricoNotificacaoRepository
{
    public HistoricoNotificacaoRepository(CobrioDbContext context) : base(context) { }

    public async Task<IEnumerable<HistoricoNotificacao>> GetByCobrancaIdAsync(Guid cobrancaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.CobrancaId == cobrancaId)
            .OrderByDescending(h => h.DataEnvio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HistoricoNotificacao>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.RegraCobrancaId == regraId)
            .OrderByDescending(h => h.DataEnvio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HistoricoNotificacao>> GetByStatusAsync(StatusNotificacao status, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.Status == status && h.DataEnvio >= dataInicio && h.DataEnvio <= dataFim)
            .OrderByDescending(h => h.DataEnvio)
            .ToListAsync(cancellationToken);
    }
}
