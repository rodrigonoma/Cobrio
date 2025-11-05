using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class CobrancaRepository : Repository<Cobranca>, ICobrancaRepository
{
    public CobrancaRepository(CobrioDbContext context) : base(context) { }

    public async Task<IEnumerable<Cobranca>> GetPendentesParaProcessarAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.RegraCobranca)
            .Where(c => c.Status == StatusCobranca.Pendente &&
                       c.DataDisparo <= DateTime.Now &&
                       c.TentativasEnvio < 5)
            .OrderBy(c => c.DataDisparo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cobranca>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.RegraCobrancaId == regraId)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cobranca>> GetByStatusAsync(StatusCobranca status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cobranca>> GetComFalhaParaRetentativaAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.RegraCobranca)
            .Where(c => c.Status == StatusCobranca.Falha && c.TentativasEnvio < 5)
            .OrderBy(c => c.AtualizadoEm)
            .ToListAsync(cancellationToken);
    }
}
