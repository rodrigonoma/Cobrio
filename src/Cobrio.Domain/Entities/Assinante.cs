using Cobrio.Domain.Enums;
using Cobrio.Domain.ValueObjects;

namespace Cobrio.Domain.Entities;

public class Assinante : BaseEntity
{
    public Guid EmpresaClienteId { get; private set; }
    public Guid PlanoOfertaId { get; private set; }

    // Dados pessoais
    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public string? CPFCNPJ { get; private set; } // Aceita CPF ou CNPJ como string
    public string? Telefone { get; private set; }

    // Assinatura
    public StatusAssinatura Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFimCiclo { get; private set; }
    public DateTime? DataCancelamento { get; private set; }
    public string? MotivoCancelamento { get; private set; }

    // Trial
    public bool EmTrial { get; private set; }
    public DateTime? DataFimTrial { get; private set; }

    // Cobrança
    public int DiaVencimento { get; private set; }
    public DateTime ProximaCobranca { get; private set; }

    // Endereco
    public Endereco? Endereco { get; private set; }

    // Navegação
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;
    public PlanoOferta PlanoOferta { get; private set; } = null!;

    private readonly List<MetodoPagamento> _metodosPagamento = new();
    public IReadOnlyCollection<MetodoPagamento> MetodosPagamento => _metodosPagamento.AsReadOnly();

    private readonly List<Fatura> _faturas = new();
    public IReadOnlyCollection<Fatura> Faturas => _faturas.AsReadOnly();

    // Construtor para EF Core
    private Assinante() { }

    public Assinante(
        Guid empresaClienteId,
        Guid planoOfertaId,
        string nome,
        Email email,
        int diaVencimento,
        string? cpfCnpj = null,
        string? telefone = null,
        Endereco? endereco = null,
        bool iniciarTrial = false,
        int diasTrial = 0)
    {
        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (planoOfertaId == Guid.Empty)
            throw new ArgumentException("PlanoOfertaId inválido", nameof(planoOfertaId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        if (diaVencimento < 1 || diaVencimento > 31)
            throw new ArgumentException("Dia de vencimento deve estar entre 1 e 31", nameof(diaVencimento));

        EmpresaClienteId = empresaClienteId;
        PlanoOfertaId = planoOfertaId;
        Nome = nome.Trim();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        CPFCNPJ = cpfCnpj?.Trim();
        Telefone = telefone?.Trim();
        Endereco = endereco;
        DiaVencimento = diaVencimento;

        DataInicio = DateTime.UtcNow;

        if (iniciarTrial && diasTrial > 0)
        {
            EmTrial = true;
            DataFimTrial = DataInicio.AddDays(diasTrial);
            Status = StatusAssinatura.Trial;
            ProximaCobranca = DataFimTrial.Value;
            DataFimCiclo = DataFimTrial.Value;
        }
        else
        {
            EmTrial = false;
            Status = StatusAssinatura.Ativo;
            ProximaCobranca = CalcularProximaCobranca(DataInicio);
            DataFimCiclo = ProximaCobranca;
        }
    }

    private DateTime CalcularProximaCobranca(DateTime dataBase)
    {
        var proximaCobranca = new DateTime(
            dataBase.Year,
            dataBase.Month,
            Math.Min(DiaVencimento, DateTime.DaysInMonth(dataBase.Year, dataBase.Month)),
            0, 0, 0, DateTimeKind.Utc);

        if (proximaCobranca <= dataBase)
        {
            proximaCobranca = proximaCobranca.AddMonths(1);
            proximaCobranca = new DateTime(
                proximaCobranca.Year,
                proximaCobranca.Month,
                Math.Min(DiaVencimento, DateTime.DaysInMonth(proximaCobranca.Year, proximaCobranca.Month)),
                0, 0, 0, DateTimeKind.Utc);
        }

        return proximaCobranca;
    }

    public void RenovarCiclo()
    {
        if (Status == StatusAssinatura.Cancelado)
            throw new InvalidOperationException("Não é possível renovar assinatura cancelada");

        ProximaCobranca = CalcularProximaCobranca(DateTime.UtcNow);
        DataFimCiclo = ProximaCobranca;
        Status = StatusAssinatura.Ativo;
        AtualizarDataModificacao();
    }

    public void MarcarComoAguardandoPagamento()
    {
        if (Status == StatusAssinatura.Cancelado)
            throw new InvalidOperationException("Assinatura já está cancelada");

        Status = StatusAssinatura.AguardandoPagamento;
        AtualizarDataModificacao();
    }

    public void MarcarComoInadimplente()
    {
        if (Status == StatusAssinatura.Cancelado)
            throw new InvalidOperationException("Assinatura já está cancelada");

        Status = StatusAssinatura.Inadimplente;
        AtualizarDataModificacao();
    }

    public void Suspender()
    {
        if (Status == StatusAssinatura.Cancelado)
            throw new InvalidOperationException("Assinatura já está cancelada");

        Status = StatusAssinatura.Suspenso;
        AtualizarDataModificacao();
    }

    public void Reativar()
    {
        if (Status == StatusAssinatura.Cancelado)
            throw new InvalidOperationException("Não é possível reativar assinatura cancelada");

        Status = StatusAssinatura.Ativo;
        AtualizarDataModificacao();
    }

    public void Cancelar(string? motivo = null)
    {
        Status = StatusAssinatura.Cancelado;
        DataCancelamento = DateTime.UtcNow;
        MotivoCancelamento = motivo;
        AtualizarDataModificacao();
    }

    public void FinalizarTrial()
    {
        if (!EmTrial)
            throw new InvalidOperationException("Assinante não está em trial");

        EmTrial = false;
        DataFimTrial = DateTime.UtcNow;
        Status = StatusAssinatura.Ativo;
        ProximaCobranca = CalcularProximaCobranca(DateTime.UtcNow);
        DataFimCiclo = ProximaCobranca;
        AtualizarDataModificacao();
    }

    public void MudarPlano(Guid novoPlanoId)
    {
        if (novoPlanoId == Guid.Empty)
            throw new ArgumentException("PlanoId inválido", nameof(novoPlanoId));

        if (Status == StatusAssinatura.Cancelado)
            throw new InvalidOperationException("Não é possível mudar plano de assinatura cancelada");

        PlanoOfertaId = novoPlanoId;
        AtualizarDataModificacao();
    }

    public void AtualizarDadosPessoais(
        string? nome = null,
        Email? email = null,
        string? telefone = null,
        Endereco? endereco = null)
    {
        if (!string.IsNullOrWhiteSpace(nome))
            Nome = nome.Trim();

        if (email != null)
            Email = email;

        if (telefone != null)
            Telefone = telefone.Trim();

        if (endereco != null)
            Endereco = endereco;

        AtualizarDataModificacao();
    }
}
