using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;

namespace Cobrio.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CobrioDbContext _context;
    private IEmpresaClienteRepository? _empresasCliente;
    private IAssinanteRepository? _assinantes;
    private IPlanoOfertaRepository? _planosOferta;
    private IFaturaRepository? _faturas;
    private IUsuarioEmpresaRepository? _usuarios;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(CobrioDbContext context)
    {
        _context = context;
    }

    public IEmpresaClienteRepository EmpresasCliente =>
        _empresasCliente ??= new EmpresaClienteRepository(_context);

    public IAssinanteRepository Assinantes =>
        _assinantes ??= new AssinanteRepository(_context);

    public IPlanoOfertaRepository PlanosOferta =>
        _planosOferta ??= new PlanoOfertaRepository(_context);

    public IFaturaRepository Faturas =>
        _faturas ??= new FaturaRepository(_context);

    public IUsuarioEmpresaRepository Usuarios =>
        _usuarios ??= new UsuarioEmpresaRepository(_context);

    public IRefreshTokenRepository RefreshTokens =>
        _refreshTokens ??= new RefreshTokenRepository(_context);

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case Microsoft.EntityFrameworkCore.EntityState.Added:
                        entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
