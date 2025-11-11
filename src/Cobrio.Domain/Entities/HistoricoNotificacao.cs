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

    // === Campos para rastreamento de eventos do Brevo ===

    // ID da mensagem no Brevo (para correlacionar eventos)
    public string? MessageIdProvedor { get; private set; }

    // Rastreamento de aberturas
    public int QuantidadeAberturas { get; private set; }
    public DateTime? DataPrimeiraAbertura { get; private set; }
    public DateTime? DataUltimaAbertura { get; private set; }
    public string? IpAbertura { get; private set; }
    public string? UserAgentAbertura { get; private set; }

    // Rastreamento de cliques
    public int QuantidadeCliques { get; private set; }
    public DateTime? DataPrimeiroClique { get; private set; }
    public DateTime? DataUltimoClique { get; private set; }
    public string? LinkClicado { get; private set; }

    // Detalhes de bounce/erro
    public string? MotivoRejeicao { get; private set; }
    public string? CodigoErroProvedor { get; private set; }

    // Navegação
    public Cobranca Cobranca { get; private set; } = null!;
    public RegraCobranca RegraCobranca { get; private set; } = null!;
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    // Timeline de mudanças de status
    private readonly List<HistoricoStatusNotificacao> _historicoStatus = new();
    public IReadOnlyCollection<HistoricoStatusNotificacao> HistoricoStatus => _historicoStatus.AsReadOnly();

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

    // === Métodos para processar eventos do webhook ===

    public void RegistrarMessageId(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return;

        MessageIdProvedor = messageId;
        AtualizarDataModificacao();
    }

    public void AtualizarStatus(StatusNotificacao novoStatus, string? motivoRejeicao = null, string? codigoErro = null)
    {
        var statusAnterior = Status;

        // Só registra mudança se for diferente
        if (statusAnterior != novoStatus)
        {
            var detalhes = new List<string>();
            if (!string.IsNullOrWhiteSpace(motivoRejeicao))
                detalhes.Add($"Motivo: {motivoRejeicao}");
            if (!string.IsNullOrWhiteSpace(codigoErro))
                detalhes.Add($"Código: {codigoErro}");

            // Registrar na timeline
            var mudanca = HistoricoStatusNotificacao.Criar(
                this.Id,
                statusAnterior,
                novoStatus,
                detalhes.Count > 0 ? string.Join(", ", detalhes) : null
            );
            _historicoStatus.Add(mudanca);

            Status = novoStatus;
        }

        if (!string.IsNullOrWhiteSpace(motivoRejeicao))
            MotivoRejeicao = motivoRejeicao;

        if (!string.IsNullOrWhiteSpace(codigoErro))
            CodigoErroProvedor = codigoErro;

        AtualizarDataModificacao();
    }

    public void RegistrarAbertura(DateTime dataAbertura, string? ip = null, string? userAgent = null)
    {
        QuantidadeAberturas++;

        if (DataPrimeiraAbertura == null)
            DataPrimeiraAbertura = dataAbertura;

        DataUltimaAbertura = dataAbertura;

        if (!string.IsNullOrWhiteSpace(ip))
            IpAbertura = ip;

        if (!string.IsNullOrWhiteSpace(userAgent))
            UserAgentAbertura = userAgent;

        // Atualizar status se ainda não foi marcado como aberto
        if (Status != StatusNotificacao.Aberto && Status != StatusNotificacao.Clicado)
        {
            var statusAnterior = Status;
            Status = StatusNotificacao.Aberto;

            // Registrar mudança na timeline
            var mudanca = HistoricoStatusNotificacao.Criar(
                this.Id,
                statusAnterior,
                StatusNotificacao.Aberto,
                $"Email aberto {QuantidadeAberturas}x",
                ip,
                userAgent
            );
            _historicoStatus.Add(mudanca);
        }

        AtualizarDataModificacao();
    }

    public void RegistrarClique(DateTime dataClique, string? link = null)
    {
        QuantidadeCliques++;

        if (DataPrimeiroClique == null)
            DataPrimeiroClique = dataClique;

        DataUltimoClique = dataClique;

        if (!string.IsNullOrWhiteSpace(link))
            LinkClicado = link;

        // Atualizar status para clicado (mais engajado que apenas aberto)
        if (Status != StatusNotificacao.Clicado)
        {
            var statusAnterior = Status;
            Status = StatusNotificacao.Clicado;

            // Registrar mudança na timeline
            var detalhes = $"Link clicado {QuantidadeCliques}x";
            if (!string.IsNullOrWhiteSpace(link))
                detalhes += $": {link}";

            var mudanca = HistoricoStatusNotificacao.Criar(
                this.Id,
                statusAnterior,
                StatusNotificacao.Clicado,
                detalhes
            );
            _historicoStatus.Add(mudanca);
        }

        AtualizarDataModificacao();
    }
}
