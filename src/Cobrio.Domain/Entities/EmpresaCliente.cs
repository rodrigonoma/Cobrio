using Cobrio.Domain.Enums;
using Cobrio.Domain.ValueObjects;

namespace Cobrio.Domain.Entities;

public class EmpresaCliente : BaseEntity
{
    public string Nome { get; private set; }
    public CNPJ CNPJ { get; private set; }
    public Email Email { get; private set; }
    public string? Telefone { get; private set; }

    // Plano Cobrio
    public int PlanoCobrioId { get; private set; }
    public DateTime DataContrato { get; private set; }
    public StatusContrato StatusContrato { get; private set; }

    // Endereco (Value Object)
    public Endereco? Endereco { get; private set; }

    // Navegação
    private readonly List<UsuarioEmpresa> _usuarios = new();
    public IReadOnlyCollection<UsuarioEmpresa> Usuarios => _usuarios.AsReadOnly();

    private readonly List<PlanoOferta> _planosOferta = new();
    public IReadOnlyCollection<PlanoOferta> PlanosOferta => _planosOferta.AsReadOnly();

    private readonly List<Assinante> _assinantes = new();
    public IReadOnlyCollection<Assinante> Assinantes => _assinantes.AsReadOnly();

    public ReguaDunningConfig? ReguaDunning { get; private set; }

    // Construtor para EF Core
    private EmpresaCliente() { }

    public EmpresaCliente(
        string nome,
        CNPJ cnpj,
        Email email,
        int planoCobrioId,
        string? telefone = null,
        Endereco? endereco = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da empresa não pode ser vazio", nameof(nome));

        Nome = nome.Trim();
        CNPJ = cnpj ?? throw new ArgumentNullException(nameof(cnpj));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PlanoCobrioId = planoCobrioId;
        Telefone = telefone?.Trim();
        Endereco = endereco;
        DataContrato = DateTime.UtcNow;
        StatusContrato = StatusContrato.Ativo;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        Nome = nome.Trim();
        AtualizarDataModificacao();
    }

    public void AtualizarEmail(Email email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        AtualizarDataModificacao();
    }

    public void AtualizarTelefone(string? telefone)
    {
        Telefone = telefone?.Trim();
        AtualizarDataModificacao();
    }

    public void AtualizarEndereco(Endereco endereco)
    {
        Endereco = endereco;
        AtualizarDataModificacao();
    }

    public void Suspender()
    {
        if (StatusContrato == StatusContrato.Cancelado)
            throw new InvalidOperationException("Não é possível suspender um contrato cancelado");

        StatusContrato = StatusContrato.Suspenso;
        AtualizarDataModificacao();
    }

    public void Ativar()
    {
        if (StatusContrato == StatusContrato.Cancelado)
            throw new InvalidOperationException("Não é possível ativar um contrato cancelado");

        StatusContrato = StatusContrato.Ativo;
        AtualizarDataModificacao();
    }

    public void Cancelar()
    {
        StatusContrato = StatusContrato.Cancelado;
        AtualizarDataModificacao();
    }
}
