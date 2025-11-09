using Cobrio.Domain.Entities;

namespace Cobrio.Domain.Interfaces;

public interface ITemplateEmailRepository : IRepository<TemplateEmail>
{
    Task<IEnumerable<TemplateEmail>> GetByEmpresaIdAsync(Guid empresaClienteId, CancellationToken cancellationToken = default);
    Task<TemplateEmail?> GetByNomeAsync(Guid empresaClienteId, string nome, CancellationToken cancellationToken = default);
}
