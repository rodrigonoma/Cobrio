using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface IHistoricoImportacaoRepository : IRepository<HistoricoImportacao>
{
    Task<IEnumerable<HistoricoImportacao>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default);
}
