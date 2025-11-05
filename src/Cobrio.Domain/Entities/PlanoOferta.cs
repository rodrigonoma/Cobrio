using Cobrio.Domain.Enums;
using Cobrio.Domain.ValueObjects;

namespace Cobrio.Domain.Entities;

public class PlanoOferta : BaseEntity
{
    public Guid EmpresaClienteId { get; private set; }
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }

    // Pricing
    public TipoCiclo TipoCiclo { get; private set; }
    public Money Valor { get; private set; }

    // Configurações
    public int PeriodoTrialDias { get; private set; }
    public int? LimiteUsuarios { get; private set; }
    public bool PermiteDowngrade { get; private set; }
    public bool PermiteUpgrade { get; private set; }
    public bool Ativo { get; private set; }

    // Navegação
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    private readonly List<Assinante> _assinantes = new();
    public IReadOnlyCollection<Assinante> Assinantes => _assinantes.AsReadOnly();

    // Construtor para EF Core
    private PlanoOferta() { }

    public PlanoOferta(
        Guid empresaClienteId,
        string nome,
        TipoCiclo tipoCiclo,
        Money valor,
        string? descricao = null,
        int periodoTrialDias = 0,
        int? limiteUsuarios = null,
        bool permiteDowngrade = true,
        bool permiteUpgrade = true)
    {
        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do plano não pode ser vazio", nameof(nome));

        if (periodoTrialDias < 0)
            throw new ArgumentException("Período de trial não pode ser negativo", nameof(periodoTrialDias));

        EmpresaClienteId = empresaClienteId;
        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        TipoCiclo = tipoCiclo;
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        PeriodoTrialDias = periodoTrialDias;
        LimiteUsuarios = limiteUsuarios;
        PermiteDowngrade = permiteDowngrade;
        PermiteUpgrade = permiteUpgrade;
        Ativo = true;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        Nome = nome.Trim();
        AtualizarDataModificacao();
    }

    public void AtualizarDescricao(string? descricao)
    {
        Descricao = descricao?.Trim();
        AtualizarDataModificacao();
    }

    public void AtualizarValor(Money novoValor)
    {
        Valor = novoValor ?? throw new ArgumentNullException(nameof(novoValor));
        AtualizarDataModificacao();
    }

    public void AtualizarConfiguracoes(
        int? limiteUsuarios,
        bool permiteDowngrade,
        bool permiteUpgrade)
    {
        LimiteUsuarios = limiteUsuarios;
        PermiteDowngrade = permiteDowngrade;
        PermiteUpgrade = permiteUpgrade;
        AtualizarDataModificacao();
    }

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

    public int ObterDiasDoCiclo()
    {
        return TipoCiclo switch
        {
            TipoCiclo.Mensal => 30,
            TipoCiclo.Trimestral => 90,
            TipoCiclo.Semestral => 180,
            TipoCiclo.Anual => 365,
            TipoCiclo.Uso => throw new InvalidOperationException("Plano de uso não tem ciclo fixo"),
            _ => throw new NotImplementedException($"TipoCiclo {TipoCiclo} não implementado")
        };
    }
}
