namespace Cobrio.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEmpresaClienteRepository EmpresasCliente { get; }
    IAssinanteRepository Assinantes { get; }
    IPlanoOfertaRepository PlanosOferta { get; }
    IFaturaRepository Faturas { get; }
    IUsuarioEmpresaRepository Usuarios { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
