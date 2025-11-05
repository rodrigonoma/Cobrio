using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class AssinanteRepository : Repository<Assinante>, IAssinanteRepository
{
    public AssinanteRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<Assinante?> GetByIdComPlanoAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PlanoOferta)
            .Include(a => a.MetodosPagamento.Where(m => m.Ativo))
            .FirstOrDefaultAsync(a => a.Id == id && a.EmpresaClienteId == empresaId, cancellationToken);
    }

    public async Task<IEnumerable<Assinante>> GetPorEmpresaAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EmpresaClienteId == empresaId)
            .Include(a => a.PlanoOferta)
            .OrderByDescending(a => a.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assinante>> GetPorStatusAsync(
        Guid empresaId,
        StatusAssinatura status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EmpresaClienteId == empresaId && a.Status == status)
            .Include(a => a.PlanoOferta)
            .OrderByDescending(a => a.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assinante>> GetInadimplentesAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EmpresaClienteId == empresaId &&
                       (a.Status == StatusAssinatura.Inadimplente ||
                        a.Status == StatusAssinatura.AguardandoPagamento))
            .Include(a => a.PlanoOferta)
            .Include(a => a.Faturas.Where(f => f.Status == StatusFatura.Falhou || f.Status == StatusFatura.Pendente))
            .OrderBy(a => a.ProximaCobranca)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assinante>> GetComCobrancaProximaAsync(
        DateTime dataLimite,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Status == StatusAssinatura.Ativo &&
                       a.ProximaCobranca <= dataLimite)
            .Include(a => a.PlanoOferta)
            .Include(a => a.MetodosPagamento.Where(m => m.Principal && m.Ativo))
            .OrderBy(a => a.ProximaCobranca)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAtivosAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(a => a.EmpresaClienteId == empresaId && a.Status == StatusAssinatura.Ativo,
                       cancellationToken);
    }
}
