using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class PlanoOfertaRepository : Repository<PlanoOferta>, IPlanoOfertaRepository
{
    public PlanoOfertaRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PlanoOferta>> GetAtivosAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.EmpresaClienteId == empresaId && p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanoOferta?> GetByIdComAssinantesAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Assinantes.Where(a => a.Status == Domain.Enums.StatusAssinatura.Ativo))
            .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaClienteId == empresaId, cancellationToken);
    }
}
