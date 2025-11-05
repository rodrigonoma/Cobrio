using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

public class HistoricoNotificacao : BaseEntity
{
    public Guid CobrancaId { get; private set; }
    public Guid RegraCobrancaId { get; private set; }
    public Guid EmpresaClienteId { get; private set; }

    // Canal utilizado para envio
    public CanalNotificacao CanalUtilizado { get; private set; }

    // Status do envio
    public StatusNotificacao Status { get; private set; }

    // Mensagem processada enviada
    public string MensagemEnviada { get; private set; }

    // Detalhes do erro caso tenha falhado
    public string? MensagemErro { get; private set; }

    // Data efetiva do envio
    public DateTime DataEnvio { get; private set; }

    // Payload original utilizado (para auditoria)
    public string PayloadUtilizado { get; private set; }

    // Resposta do provedor (SendGrid, Twilio, etc)
    public string? RespostaProvedor { get; private set; }

    // Navegação
    public Cobranca Cobranca { get; private set; } = null!;
    public RegraCobranca RegraCobranca { get; private set; } = null!;
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    // Construtor para EF Core
    private HistoricoNotificacao() { }

    public HistoricoNotificacao(
        Guid cobrancaId,
        Guid regraCobrancaId,
        Guid empresaClienteId,
        CanalNotificacao canalUtilizado,
        string mensagemEnviada,
        string payloadUtilizado,
        StatusNotificacao status,
        string? mensagemErro = null,
        string? respostaProvedor = null)
    {
        if (cobrancaId == Guid.Empty)
            throw new ArgumentException("CobrancaId inválido", nameof(cobrancaId));

        if (regraCobrancaId == Guid.Empty)
            throw new ArgumentException("RegraCobrancaId inválido", nameof(regraCobrancaId));

        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(mensagemEnviada))
            throw new ArgumentException("MensagemEnviada não pode ser vazia", nameof(mensagemEnviada));

        if (string.IsNullOrWhiteSpace(payloadUtilizado))
            throw new ArgumentException("PayloadUtilizado não pode ser vazio", nameof(payloadUtilizado));

        CobrancaId = cobrancaId;
        RegraCobrancaId = regraCobrancaId;
        EmpresaClienteId = empresaClienteId;
        CanalUtilizado = canalUtilizado;
        MensagemEnviada = mensagemEnviada;
        PayloadUtilizado = payloadUtilizado;
        Status = status;
        MensagemErro = mensagemErro;
        RespostaProvedor = respostaProvedor;
        DataEnvio = DateTime.UtcNow;
    }

    public static HistoricoNotificacao CriarSucesso(
        Guid cobrancaId,
        Guid regraCobrancaId,
        Guid empresaClienteId,
        CanalNotificacao canalUtilizado,
        string mensagemEnviada,
        string payloadUtilizado,
        string? respostaProvedor = null)
    {
        return new HistoricoNotificacao(
            cobrancaId,
            regraCobrancaId,
            empresaClienteId,
            canalUtilizado,
            mensagemEnviada,
            payloadUtilizado,
            StatusNotificacao.Sucesso,
            respostaProvedor: respostaProvedor);
    }

    public static HistoricoNotificacao CriarFalha(
        Guid cobrancaId,
        Guid regraCobrancaId,
        Guid empresaClienteId,
        CanalNotificacao canalUtilizado,
        string mensagemEnviada,
        string payloadUtilizado,
        string mensagemErro,
        string? respostaProvedor = null)
    {
        return new HistoricoNotificacao(
            cobrancaId,
            regraCobrancaId,
            empresaClienteId,
            canalUtilizado,
            mensagemEnviada,
            payloadUtilizado,
            StatusNotificacao.Falha,
            mensagemErro,
            respostaProvedor);
    }
}
