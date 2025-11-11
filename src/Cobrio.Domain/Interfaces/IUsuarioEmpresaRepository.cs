using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface IUsuarioEmpresaRepository : IRepository<UsuarioEmpresa>
{
    Task<UsuarioEmpresa?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UsuarioEmpresa?> GetByEmailWithEmpresaAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsuarioEmpresa>> GetByEmpresaIdAsync(Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<UsuarioEmpresa?> GetByIdAndEmpresaAsync(Guid id, Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<int> ContarUsuariosAtivosPorEmpresaAsync(Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsuarioEmpresa>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
