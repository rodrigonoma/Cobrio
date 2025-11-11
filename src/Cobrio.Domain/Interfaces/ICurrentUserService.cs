using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

/// <summary>
/// Service para obter informações do usuário autenticado atual
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID do usuário autenticado (do JWT)
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Email do usuário autenticado
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Nome do usuário autenticado
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Perfil do usuário autenticado
    /// </summary>
    PerfilUsuario? Perfil { get; }

    /// <summary>
    /// ID da empresa do usuário autenticado
    /// </summary>
    Guid? EmpresaClienteId { get; }

    /// <summary>
    /// Verifica se o usuário é proprietário
    /// </summary>
    bool EhProprietario { get; }

    /// <summary>
    /// Verifica se o usuário está autenticado
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Verifica se o usuário pode visualizar dados de outro usuário
    /// baseado nas regras de negócio:
    /// - Operador: só vê os próprios
    /// - Admin: vê operadores + próprios (não vê outros admins)
    /// - Proprietário: vê tudo
    /// </summary>
    bool PodeVisualizarDadosDe(Guid? usuarioCriacaoId, PerfilUsuario? perfilCriador);
}
