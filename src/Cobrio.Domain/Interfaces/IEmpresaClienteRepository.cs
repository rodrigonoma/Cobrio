using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface IEmpresaClienteRepository : IRepository<EmpresaCliente>
{
    Task<EmpresaCliente?> GetByCNPJAsync(string cnpj, CancellationToken cancellationToken = default);
    Task<EmpresaCliente?> GetComReguaDunningAsync(Guid id, CancellationToken cancellationToken = default);
}
