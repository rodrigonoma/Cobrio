using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

public interface IHistoricoNotificacaoRepository : IRepository<HistoricoNotificacao>
{
    Task<IEnumerable<HistoricoNotificacao>> GetByCobrancaIdAsync(Guid cobrancaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricoNotificacao>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricoNotificacao>> GetByStatusAsync(StatusNotificacao status, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
}
