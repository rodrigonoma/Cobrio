using Cobrio.Application.DTOs.RegraCobranca;

namespace Cobrio.Application.Interfaces;

public interface IRegraCobrancaService
{
    Task<RegraCobrancaResponse> CreateAsync(Guid empresaClienteId, CreateRegraCobrancaRequest request, CancellationToken cancellationToken = default);
    Task<RegraCobrancaResponse> UpdateAsync(Guid empresaClienteId, Guid id, UpdateRegraCobrancaRequest request, CancellationToken cancellationToken = default);
    Task<RegraCobrancaResponse> GetByIdAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RegraCobrancaResponse>> GetAllAsync(Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RegraCobrancaResponse>> GetRegrasAtivasAsync(Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default);
    Task<RegraCobrancaResponse> AtivarAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default);
    Task<RegraCobrancaResponse> DesativarAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default);
    Task<RegraCobrancaResponse> RegenerarTokenAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default);
}
