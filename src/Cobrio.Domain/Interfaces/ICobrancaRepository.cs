using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

public interface ICobrancaRepository : IRepository<Cobranca>
{
    Task<IEnumerable<Cobranca>> GetPendentesParaProcessarAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Cobranca>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cobranca>> GetByStatusAsync(StatusCobranca status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cobranca>> GetComFalhaParaRetentativaAsync(CancellationToken cancellationToken = default);
}
