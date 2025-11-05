using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

public interface IAssinanteRepository : IRepository<Assinante>
{
    Task<Assinante?> GetByIdComPlanoAsync(Guid id, Guid empresaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assinante>> GetPorEmpresaAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assinante>> GetPorStatusAsync(Guid empresaId, StatusAssinatura status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assinante>> GetInadimplentesAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assinante>> GetComCobrancaProximaAsync(DateTime dataLimite, CancellationToken cancellationToken = default);
    Task<int> ContarAtivosAsync(Guid empresaId, CancellationToken cancellationToken = default);
}
