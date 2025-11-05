using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Application.Interfaces;

public interface IPermissaoService
{
    Task<IEnumerable<Modulo>> GetModulosAtivosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Acao>> GetAcoesAtivasAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<PermissaoPerfil>> GetPermissoesByPerfilAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        CancellationToken cancellationToken = default);

    Task<bool> TemPermissaoAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        string moduloChave,
        string acaoChave,
        CancellationToken cancellationToken = default);

    Task ConfigurarPermissoesAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        Dictionary<Guid, Dictionary<Guid, bool>> permissoes,
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task LimparCachePermissoesAsync(Guid empresaClienteId, PerfilUsuario perfil);
}
