using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface IRegraCobrancaRepository : IRepository<RegraCobranca>
{
    Task<IEnumerable<RegraCobranca>> GetByEmpresaIdAsync(Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RegraCobranca>> GetRegrasAtivasAsync(Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<RegraCobranca?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
}
