using Cobrio.Application.DTOs.PlanoOferta;

namespace Cobrio.Application.Interfaces;

public interface IPlanoOfertaService
{
    Task<PlanoOfertaResponse> CriarAsync(CreatePlanoOfertaRequest request, CancellationToken cancellationToken = default);
    Task<PlanoOfertaResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanoOfertaResponse>> ListarAsync(bool? apenasAtivos = null, CancellationToken cancellationToken = default);
    Task<PlanoOfertaResponse> AtualizarAsync(Guid id, UpdatePlanoOfertaRequest request, CancellationToken cancellationToken = default);
    Task AtivarAsync(Guid id, CancellationToken cancellationToken = default);
    Task DesativarAsync(Guid id, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}
