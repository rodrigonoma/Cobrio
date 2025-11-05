using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class EmpresaClienteRepository : Repository<EmpresaCliente>, IEmpresaClienteRepository
{
    public EmpresaClienteRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<EmpresaCliente?> GetByCNPJAsync(
        string cnpj,
        CancellationToken cancellationToken = default)
    {
        // Remove formatação do CNPJ
        var cnpjLimpo = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();

        return await _context.SetIgnoreQueryFilters<EmpresaCliente>()
            .FirstOrDefaultAsync(e => e.CNPJ.Numero == cnpjLimpo, cancellationToken);
    }

    public async Task<EmpresaCliente?> GetComReguaDunningAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.ReguaDunning)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
