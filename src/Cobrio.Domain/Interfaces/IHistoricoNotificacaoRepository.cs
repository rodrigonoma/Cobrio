using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

public interface IHistoricoNotificacaoRepository : IRepository<HistoricoNotificacao>
{
    Task<IEnumerable<HistoricoNotificacao>> GetByCobrancaIdAsync(Guid cobrancaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricoNotificacao>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricoNotificacao>> GetByStatusAsync(StatusNotificacao status, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<HistoricoNotificacao?> GetByMessageIdProvedor(string messageId, CancellationToken cancellationToken = default);
    Task<HistoricoNotificacao?> GetByEmailEDataAsync(string email, DateTime dataReferencia, int toleranciaMinutos = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricoNotificacao>> GetByFiltrosAsync(Guid empresaClienteId, DateTime? dataInicio, DateTime? dataFim, StatusNotificacao? status, string? emailDestinatario, CancellationToken cancellationToken = default);
}
