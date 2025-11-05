using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveByUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task RevokeAllByUsuarioAsync(Guid usuarioId, string revokedByIp, CancellationToken cancellationToken = default);
}
