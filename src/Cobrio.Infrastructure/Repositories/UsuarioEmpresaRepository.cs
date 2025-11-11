using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class UsuarioEmpresaRepository : Repository<UsuarioEmpresa>, IUsuarioEmpresaRepository
{
    public UsuarioEmpresaRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<UsuarioEmpresa?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailObj = new Domain.ValueObjects.Email(email);
        return await _context.UsuariosEmpresa
            .IgnoreQueryFilters() // Necessário para login (sem TenantId)
            .FirstOrDefaultAsync(u => u.Email == emailObj, cancellationToken);
    }

    public async Task<UsuarioEmpresa?> GetByEmailWithEmpresaAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailObj = new Domain.ValueObjects.Email(email);
        return await _context.UsuariosEmpresa
            .IgnoreQueryFilters() // Necessário para login (sem TenantId)
            .Include(u => u.EmpresaCliente)
            .FirstOrDefaultAsync(u => u.Email == emailObj, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid empresaClienteId, CancellationToken cancellationToken = default)
    {
        var emailObj = new Domain.ValueObjects.Email(email);
        return await _context.UsuariosEmpresa
            .AnyAsync(u => u.Email == emailObj && u.EmpresaClienteId == empresaClienteId, cancellationToken);
    }

    public async Task<IEnumerable<UsuarioEmpresa>> GetByEmpresaIdAsync(Guid empresaClienteId, CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosEmpresa
            .Where(u => u.EmpresaClienteId == empresaClienteId)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioEmpresa?> GetByIdAndEmpresaAsync(Guid id, Guid empresaClienteId, CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosEmpresa
            .FirstOrDefaultAsync(u => u.Id == id && u.EmpresaClienteId == empresaClienteId, cancellationToken);
    }

    public async Task<int> ContarUsuariosAtivosPorEmpresaAsync(Guid empresaClienteId, CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosEmpresa
            .CountAsync(u => u.EmpresaClienteId == empresaClienteId && u.Ativo, cancellationToken);
    }

    public async Task<IEnumerable<UsuarioEmpresa>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (!ids.Any())
            return Enumerable.Empty<UsuarioEmpresa>();

        return await _context.UsuariosEmpresa
            .IgnoreQueryFilters() // Para buscar usuários de outras empresas (para auditoria)
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);
    }
}
