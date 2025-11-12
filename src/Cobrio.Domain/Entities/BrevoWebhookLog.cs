namespace Cobrio.Domain.Entities;

/// <summary>
/// Log de todos os webhooks recebidos da Brevo para auditoria e debug
/// </summary>
public class BrevoWebhookLog : BaseEntity
{
    // Dados do evento
    public string EventoTipo { get; private set; }
    public string Email { get; private set; }
    public string? MessageId { get; private set; }
    public long BrevoEventId { get; private set; }

    // Payload completo em JSON
    public string PayloadCompleto { get; private set; }

    // Informações da requisição HTTP
    public string? EnderecoIp { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Headers { get; private set; }

    // Resultado do processamento
    public bool ProcessadoComSucesso { get; private set; }
    public string? MensagemErro { get; private set; }
    public Guid? HistoricoNotificacaoId { get; private set; }

    // Timestamp do evento
    public DateTime DataEvento { get; private set; }
    public DateTime DataRecebimento { get; private set; }

    // Construtor para EF Core
    private BrevoWebhookLog()
    {
        EventoTipo = string.Empty;
        Email = string.Empty;
        PayloadCompleto = string.Empty;
    }

    public BrevoWebhookLog(
        string eventoTipo,
        string email,
        string? messageId,
        long brevoEventId,
        string payloadCompleto,
        DateTime dataEvento,
        string? enderecoIp = null,
        string? userAgent = null,
        string? headers = null)
    {
        if (string.IsNullOrWhiteSpace(eventoTipo))
            throw new ArgumentException("EventoTipo não pode ser vazio", nameof(eventoTipo));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));

        if (string.IsNullOrWhiteSpace(payloadCompleto))
            throw new ArgumentException("PayloadCompleto não pode ser vazio", nameof(payloadCompleto));

        EventoTipo = eventoTipo;
        Email = email;
        MessageId = messageId;
        BrevoEventId = brevoEventId;
        PayloadCompleto = payloadCompleto;
        DataEvento = dataEvento;
        DataRecebimento = DateTime.UtcNow;
        EnderecoIp = enderecoIp;
        UserAgent = userAgent;
        Headers = headers;
        ProcessadoComSucesso = false;
    }

    public void MarcarComoProcessado(Guid? historicoNotificacaoId = null)
    {
        ProcessadoComSucesso = true;
        HistoricoNotificacaoId = historicoNotificacaoId;
        MensagemErro = null;
        AtualizarDataModificacao();
    }

    public void MarcarComoFalha(string mensagemErro)
    {
        ProcessadoComSucesso = false;
        MensagemErro = mensagemErro;
        AtualizarDataModificacao();
    }
}
