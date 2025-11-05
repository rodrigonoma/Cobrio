using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class HistoricoImportacaoRepository : Repository<HistoricoImportacao>, IHistoricoImportacaoRepository
{
    public HistoricoImportacaoRepository(CobrioDbContext context) : base(context) { }

    public async Task<IEnumerable<HistoricoImportacao>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.RegraCobranca)
            .Include(h => h.Usuario)
            .Where(h => h.RegraCobrancaId == regraId)
            .OrderByDescending(h => h.DataImportacao)
            .ToListAsync(cancellationToken);
    }
}
