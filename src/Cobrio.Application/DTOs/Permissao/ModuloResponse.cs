namespace Cobrio.Application.DTOs.Permissao;

public class ModuloResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public string Rota { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
}
