using System.Text.Json.Serialization;

namespace Cobrio.Application.DTOs.Brevo;

/// <summary>
/// DTO para receber eventos do webhook do Brevo
/// Documentação: https://developers.brevo.com/docs/transactional-webhooks
/// </summary>
public class BrevoWebhookEvent
{
    /// <summary>
    /// Tipo do evento: request, delivered, opened, clicked, hard_bounce, soft_bounce,
    /// invalid_email, deferred, blocked, complaint, unsubscribe, error
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>
    /// Email do destinatário
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ID numérico da mensagem no Brevo
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Data do evento no formato "YYYY-MM-DD HH:mm:ss"
    /// </summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    /// <summary>
    /// Timestamp Unix do evento
    /// </summary>
    [JsonPropertyName("ts")]
    public long? Ts { get; set; }

    /// <summary>
    /// Message-ID do email (formato RFC 2822)
    /// </summary>
    [JsonPropertyName("message-id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// Timestamp Unix do evento
    /// </summary>
    [JsonPropertyName("ts_event")]
    public long? TsEvent { get; set; }

    /// <summary>
    /// Assunto do email enviado
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// Tag personalizada (se enviado com o email)
    /// </summary>
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// IP de envio
    /// </summary>
    [JsonPropertyName("sending_ip")]
    public string? SendingIp { get; set; }

    /// <summary>
    /// Timestamp em milissegundos
    /// </summary>
    [JsonPropertyName("ts_epoch")]
    public long? TsEpoch { get; set; }

    /// <summary>
    /// Array de tags
    /// </summary>
    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    // === Campos específicos para evento "opened" ===

    /// <summary>
    /// IP de onde o email foi aberto
    /// </summary>
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    /// <summary>
    /// User-Agent do navegador/cliente que abriu o email
    /// </summary>
    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; set; }

    // === Campos específicos para evento "clicked" ===

    /// <summary>
    /// URL do link clicado
    /// </summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    // === Campos específicos para bounces e erros ===

    /// <summary>
    /// Motivo da rejeição/bounce
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// Código de erro SMTP
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Template ID (se usado)
    /// </summary>
    [JsonPropertyName("template_id")]
    public long? TemplateId { get; set; }

    /// <summary>
    /// Parâmetros do template (JSON)
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, object>? Params { get; set; }
}
