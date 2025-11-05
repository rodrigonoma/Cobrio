using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

/// <summary>
/// Representa uma ação que pode ser executada em um módulo (ex: Criar, Editar, Deletar)
/// </summary>
public class Acao : BaseEntity
{
    // Construtor para EF Core
    private Acao() { }

    public Acao(
        string nome,
        string chave,
        string descricao,
        TipoAcao tipoAcao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da ação não pode ser vazio", nameof(nome));

        if (string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("Chave da ação não pode ser vazia", nameof(chave));

        Nome = nome.Trim();
        Chave = chave.Trim().ToLower();
        Descricao = descricao?.Trim() ?? string.Empty;
        TipoAcao = tipoAcao;
        Ativa = true;
    }

    public string Nome { get; private set; } = string.Empty; // ex: "Visualizar", "Criar"
    public string Chave { get; private set; } = string.Empty; // ex: "read", "create"
    public string Descricao { get; private set; } = string.Empty;
    public TipoAcao TipoAcao { get; private set; } // Menu, CRUD, Especial
    public bool Ativa { get; private set; }

    // Relacionamentos
    public ICollection<PermissaoPerfil> Permissoes { get; private set; } = new List<PermissaoPerfil>();

    public void Ativar()
    {
        Ativa = true;
        AtualizarDataModificacao();
    }

    public void Desativar()
    {
        Ativa = false;
        AtualizarDataModificacao();
    }
}
