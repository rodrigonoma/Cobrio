using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class RegraCobrancaRepository : Repository<RegraCobranca>, IRegraCobrancaRepository
{
    public RegraCobrancaRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RegraCobranca>> GetByEmpresaIdAsync(
        Guid empresaClienteId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EmpresaClienteId == empresaClienteId)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RegraCobranca>> GetRegrasAtivasAsync(
        Guid empresaClienteId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EmpresaClienteId == empresaClienteId && r.Ativa)
            .OrderBy(r => r.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<RegraCobranca?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.TokenWebhook == token, cancellationToken);
    }
}
