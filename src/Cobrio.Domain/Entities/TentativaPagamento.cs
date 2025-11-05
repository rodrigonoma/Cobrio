using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

public class TentativaPagamento : BaseEntity
{
    public Guid FaturaId { get; private set; }
    public Guid EmpresaClienteId { get; private set; }

    public int NumeroTentativa { get; private set; }
    public DateTime DataTentativa { get; private set; }

    public ResultadoTentativa Resultado { get; private set; }
    public string? CodigoErro { get; private set; }
    public string? MensagemErro { get; private set; }

    // Gateway
    public string? TransacaoIdGateway { get; private set; }
    public string? GatewayProvider { get; private set; }

    // Navegação
    public Fatura Fatura { get; private set; } = null!;
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    // Construtor para EF Core
    private TentativaPagamento() { }

    public TentativaPagamento(
        Guid faturaId,
        Guid empresaClienteId,
        int numeroTentativa,
        string? gatewayProvider = null)
    {
        if (faturaId == Guid.Empty)
            throw new ArgumentException("FaturaId inválido", nameof(faturaId));

        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (numeroTentativa <= 0)
            throw new ArgumentException("Número da tentativa deve ser maior que zero", nameof(numeroTentativa));

        FaturaId = faturaId;
        EmpresaClienteId = empresaClienteId;
        NumeroTentativa = numeroTentativa;
        DataTentativa = DateTime.UtcNow;
        Resultado = ResultadoTentativa.Processando;
        GatewayProvider = gatewayProvider;
    }

    public void MarcarComoSucesso(string? transacaoId = null)
    {
        Resultado = ResultadoTentativa.Sucesso;
        TransacaoIdGateway = transacaoId;
        AtualizarDataModificacao();
    }

    public void MarcarComoFalha(string? codigoErro = null, string? mensagemErro = null, string? transacaoId = null)
    {
        Resultado = ResultadoTentativa.Falha;
        CodigoErro = codigoErro;
        MensagemErro = mensagemErro;
        TransacaoIdGateway = transacaoId;
        AtualizarDataModificacao();
    }

    public void Cancelar()
    {
        Resultado = ResultadoTentativa.Cancelado;
        AtualizarDataModificacao();
    }
}
