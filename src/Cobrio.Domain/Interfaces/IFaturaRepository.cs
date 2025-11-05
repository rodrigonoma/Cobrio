using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

public interface IFaturaRepository : IRepository<Fatura>
{
    Task<Fatura?> GetByNumeroFaturaAsync(string numeroFatura, Guid empresaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Fatura>> GetPorAssinanteAsync(Guid assinanteId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Fatura>> GetPorStatusAsync(Guid empresaId, StatusFatura status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Fatura>> GetVencidasAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Fatura>> GetFalhadasAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<decimal> ObterReceitaMensalAsync(Guid empresaId, int mes, int ano, CancellationToken cancellationToken = default);
    Task<string> GerarProximoNumeroFaturaAsync(Guid empresaId, CancellationToken cancellationToken = default);
}
