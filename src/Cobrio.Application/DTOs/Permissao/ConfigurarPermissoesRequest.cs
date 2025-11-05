using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.Permissao;

public class ConfigurarPermissoesRequest
{
    public PerfilUsuario PerfilUsuario { get; set; }

    /// <summary>
    /// Dicionário: ModuloId => Dicionário: AcaoId => Permitido
    /// </summary>
    public Dictionary<Guid, Dictionary<Guid, bool>> Permissoes { get; set; } = new();
}
