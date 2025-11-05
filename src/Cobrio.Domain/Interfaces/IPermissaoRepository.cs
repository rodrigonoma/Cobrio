using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

public interface IPermissaoRepository
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

    Task<PermissaoPerfil?> GetPermissaoAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        Guid moduloId,
        Guid acaoId,
        CancellationToken cancellationToken = default);

    Task UpsertPermissaoAsync(
        PermissaoPerfil permissao,
        CancellationToken cancellationToken = default);

    Task UpsertPermissoesEmLoteAsync(
        IEnumerable<PermissaoPerfil> permissoes,
        CancellationToken cancellationToken = default);
}
