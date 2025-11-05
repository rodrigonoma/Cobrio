namespace Cobrio.Domain.Entities;

/// <summary>
/// Representa um módulo/funcionalidade do sistema (ex: Usuários, Planos, etc)
/// </summary>
public class Modulo : BaseEntity
{
    // Construtor para EF Core
    private Modulo() { }

    public Modulo(
        string nome,
        string chave,
        string descricao,
        string icone,
        string rota,
        int ordem)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do módulo não pode ser vazio", nameof(nome));

        if (string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("Chave do módulo não pode ser vazia", nameof(chave));

        Nome = nome.Trim();
        Chave = chave.Trim().ToLower();
        Descricao = descricao?.Trim() ?? string.Empty;
        Icone = icone?.Trim() ?? "pi-circle";
        Rota = rota?.Trim() ?? string.Empty;
        Ordem = ordem;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Chave { get; private set; } = string.Empty; // ex: "usuarios", "planos"
    public string Descricao { get; private set; } = string.Empty;
    public string Icone { get; private set; } = string.Empty; // ex: "pi-user"
    public string Rota { get; private set; } = string.Empty; // ex: "/usuarios"
    public int Ordem { get; private set; } // Ordem no menu
    public bool Ativo { get; private set; }

    // Relacionamentos
    public ICollection<PermissaoPerfil> Permissoes { get; private set; } = new List<PermissaoPerfil>();

    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }

    public void AtualizarOrdem(int novaOrdem)
    {
        Ordem = novaOrdem;
        AtualizarDataModificacao();
    }
}
