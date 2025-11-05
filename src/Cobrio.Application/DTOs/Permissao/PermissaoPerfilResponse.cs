using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.Permissao;

public class PermissaoPerfilResponse
{
    public PerfilUsuario PerfilUsuario { get; set; }
    public List<ModuloPermissaoResponse> Modulos { get; set; } = new();
}

public class ModuloPermissaoResponse
{
    public Guid ModuloId { get; set; }
    public string ModuloNome { get; set; } = string.Empty;
    public string ModuloChave { get; set; } = string.Empty;
    public string ModuloIcone { get; set; } = string.Empty;
    public string ModuloRota { get; set; } = string.Empty;
    public List<AcaoPermissaoResponse> Acoes { get; set; } = new();
}

public class AcaoPermissaoResponse
{
    public Guid AcaoId { get; set; }
    public string AcaoNome { get; set; } = string.Empty;
    public string AcaoChave { get; set; } = string.Empty;
    public TipoAcao TipoAcao { get; set; }
    public bool Permitido { get; set; }
}
