using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.UsuarioEmpresa)
                .ThenInclude(u => u.EmpresaCliente)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UsuarioEmpresaId == usuarioId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeAllByUsuarioAsync(Guid usuarioId, string revokedByIp, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UsuarioEmpresaId == usuarioId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(revokedByIp);
        }
    }
}
