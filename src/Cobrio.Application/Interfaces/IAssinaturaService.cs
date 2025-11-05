using Cobrio.Application.DTOs.Assinatura;

namespace Cobrio.Application.Interfaces;

public interface IAssinaturaService
{
    Task<AssinaturaResponse> CriarAssinaturaAsync(CreateAssinaturaRequest request, CancellationToken cancellationToken = default);
    Task<AssinaturaResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AssinaturaResponse>> ListarAsync(int pagina = 1, int tamanhoPagina = 50, CancellationToken cancellationToken = default);
    Task<AssinaturaResponse> AtualizarAsync(Guid id, UpdateAssinaturaRequest request, CancellationToken cancellationToken = default);
    Task<AssinaturaResponse> CancelarAsync(Guid id, CancelarAssinaturaRequest request, CancellationToken cancellationToken = default);
    Task<AssinaturaResponse> SuspenderAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default);
    Task<AssinaturaResponse> ReativarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AssinaturaResponse> AlterarPlanoAsync(Guid id, Guid novoPlanoId, CancellationToken cancellationToken = default);
}
