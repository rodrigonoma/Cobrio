using Cobrio.Domain.Enums;
using Cobrio.Domain.ValueObjects;

namespace Cobrio.Domain.Entities;

public class Fatura : BaseEntity
{
    public Guid AssinanteId { get; private set; }
    public Guid EmpresaClienteId { get; private set; }

    public string NumeroFatura { get; private set; }

    // Valores
    public Money ValorBruto { get; private set; }
    public Money Desconto { get; private set; }
    public Money Impostos { get; private set; }
    public Money ValorLiquido { get; private set; }

    // Datas
    public DateTime DataEmissao { get; private set; }
    public DateTime DataVencimento { get; private set; }
    public DateTime? DataPagamento { get; private set; }

    // Status
    public StatusFatura Status { get; private set; }

    // Referências
    public Guid? MetodoPagamentoId { get; private set; }
    public string? TransacaoIdGateway { get; private set; }

    // Observações
    public string? Observacoes { get; private set; }
    public string? LinkBoleto { get; private set; }
    public string? QrCodePix { get; private set; }

    // Navegação
    public Assinante Assinante { get; private set; } = null!;
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;
    public MetodoPagamento? MetodoPagamento { get; private set; }

    private readonly List<ItemFatura> _itens = new();
    public IReadOnlyCollection<ItemFatura> Itens => _itens.AsReadOnly();

    private readonly List<TentativaPagamento> _tentativas = new();
    public IReadOnlyCollection<TentativaPagamento> Tentativas => _tentativas.AsReadOnly();

    // Construtor para EF Core
    private Fatura() { }

    public Fatura(
        Guid assinanteId,
        Guid empresaClienteId,
        string numeroFatura,
        Money valorBruto,
        DateTime dataVencimento,
        Money? desconto = null,
        Money? impostos = null,
        string? observacoes = null)
    {
        if (assinanteId == Guid.Empty)
            throw new ArgumentException("AssinanteId inválido", nameof(assinanteId));

        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(numeroFatura))
            throw new ArgumentException("Número da fatura não pode ser vazio", nameof(numeroFatura));

        AssinanteId = assinanteId;
        EmpresaClienteId = empresaClienteId;
        NumeroFatura = numeroFatura.Trim();
        ValorBruto = valorBruto ?? throw new ArgumentNullException(nameof(valorBruto));
        Desconto = desconto ?? Money.Zero(valorBruto.Moeda);
        Impostos = impostos ?? Money.Zero(valorBruto.Moeda);
        ValorLiquido = ValorBruto.Subtract(Desconto).Add(Impostos);
        DataEmissao = DateTime.UtcNow;
        DataVencimento = dataVencimento;
        Status = StatusFatura.Pendente;
        Observacoes = observacoes;
    }

    public void AdicionarItem(ItemFatura item)
    {
        if (Status == StatusFatura.Pago)
            throw new InvalidOperationException("Não é possível adicionar itens a uma fatura paga");

        _itens.Add(item);
        AtualizarDataModificacao();
    }

    public void RegistrarTentativaPagamento(TentativaPagamento tentativa)
    {
        _tentativas.Add(tentativa);
        AtualizarDataModificacao();
    }

    public void MarcarComoPago(DateTime dataPagamento, string? transacaoId = null)
    {
        if (Status == StatusFatura.Cancelado)
            throw new InvalidOperationException("Não é possível marcar como pago uma fatura cancelada");

        Status = StatusFatura.Pago;
        DataPagamento = dataPagamento;
        TransacaoIdGateway = transacaoId;
        AtualizarDataModificacao();
    }

    public void MarcarComoFalhou(string? transacaoId = null)
    {
        if (Status == StatusFatura.Cancelado)
            throw new InvalidOperationException("Fatura já está cancelada");

        Status = StatusFatura.Falhou;
        TransacaoIdGateway = transacaoId;
        AtualizarDataModificacao();
    }

    public void MarcarComoAguardandoPagamento(string? linkBoleto = null, string? qrCodePix = null)
    {
        Status = StatusFatura.AguardandoPagamento;
        LinkBoleto = linkBoleto;
        QrCodePix = qrCodePix;
        AtualizarDataModificacao();
    }

    public void Cancelar()
    {
        if (Status == StatusFatura.Pago)
            throw new InvalidOperationException("Não é possível cancelar uma fatura paga");

        Status = StatusFatura.Cancelado;
        AtualizarDataModificacao();
    }

    public void Reembolsar()
    {
        if (Status != StatusFatura.Pago)
            throw new InvalidOperationException("Apenas faturas pagas podem ser reembolsadas");

        Status = StatusFatura.Reembolsado;
        AtualizarDataModificacao();
    }

    public void AssociarMetodoPagamento(Guid metodoPagamentoId)
    {
        MetodoPagamentoId = metodoPagamentoId;
        AtualizarDataModificacao();
    }

    public bool EstaVencida() => DateTime.UtcNow > DataVencimento && Status != StatusFatura.Pago;

    public int DiasAteVencimento() => (DataVencimento - DateTime.UtcNow).Days;

    public int DiasAposVencimento()
    {
        if (!EstaVencida()) return 0;
        return (DateTime.UtcNow - DataVencimento).Days;
    }
}
