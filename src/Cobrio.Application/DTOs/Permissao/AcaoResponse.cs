using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.Permissao;

public class AcaoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TipoAcao TipoAcao { get; set; }
    public bool Ativa { get; set; }
}
