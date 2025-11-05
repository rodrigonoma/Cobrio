using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface IPlanoOfertaRepository : IRepository<PlanoOferta>
{
    Task<IEnumerable<PlanoOferta>> GetAtivosAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<PlanoOferta?> GetByIdComAssinantesAsync(Guid id, Guid empresaId, CancellationToken cancellationToken = default);
}
