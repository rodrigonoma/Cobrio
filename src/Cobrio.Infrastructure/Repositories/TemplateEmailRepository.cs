using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class TemplateEmailRepository : Repository<TemplateEmail>, ITemplateEmailRepository
{
    public TemplateEmailRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TemplateEmail>> GetByEmpresaIdAsync(
        Guid empresaClienteId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.EmpresaClienteId == empresaClienteId)
            .OrderBy(t => t.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateEmail?> GetByNomeAsync(
        Guid empresaClienteId,
        string nome,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.EmpresaClienteId == empresaClienteId && t.Nome == nome, cancellationToken);
    }
}
