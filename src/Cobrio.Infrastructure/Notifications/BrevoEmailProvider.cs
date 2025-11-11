using Cobrio.Application.Configuration;
using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cobrio.Infrastructure.Notifications;

public class BrevoEmailProvider : IEmailProvider
{
    private readonly HttpClient _httpClient;
    private readonly BrevoSettings _settings;
    private readonly ILogger<BrevoEmailProvider> _logger;
    private readonly IHistoricoNotificacaoRepository _historicoRepository;

    public CanalNotificacao TipoCanal => CanalNotificacao.Email;
    public string ProviderName => "Brevo";

    public BrevoEmailProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<BrevoSettings> settings,
        ILogger<BrevoEmailProvider> logger,
        IHistoricoNotificacaoRepository historicoRepository)
    {
        _settings = settings.Value;
        _logger = logger;
        _historicoRepository = historicoRepository;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.brevo.com/v3/");
        _httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
    }

    public async Task<NotificationResult> EnviarAsync(
        string destinatario,
        string mensagem,
        string? assunto = null,
        CancellationToken cancellationToken = default)
    {
        return await EnviarEmailAsync(
            destinatario,
            assunto ?? "Notificação Cobrio",
            mensagem,
            isHtml: true,
            cancellationToken: cancellationToken);
    }

    public async Task<NotificationResult> EnviarEmailAsync(
        string destinatario,
        string assunto,
        string corpo,
        bool isHtml = true,
        string? remetenteEmail = null,
        string? remetenteNome = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fromEmail = remetenteEmail ?? _settings.FromEmail;
            var fromName = remetenteNome ?? _settings.FromName;

            _logger.LogInformation(
                "Enviando email via Brevo para {Destinatario} de {FromName} <{FromEmail}>",
                destinatario, fromName, fromEmail);

            var request = new BrevoSendEmailRequest
            {
                Sender = new BrevoEmailAddress { Email = fromEmail, Name = fromName },
                To = new[] { new BrevoEmailAddress { Email = destinatario } },
                Subject = assunto,
                HtmlContent = isHtml ? corpo : null,
                TextContent = isHtml ? null : corpo
            };

            if (!string.IsNullOrWhiteSpace(replyTo))
            {
                request.ReplyTo = new BrevoEmailAddress { Email = replyTo };
            }

            var response = await _httpClient.PostAsJsonAsync("smtp/email", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BrevoSendEmailResponse>(cancellationToken);
                var messageId = result?.MessageId;

                _logger.LogInformation(
                    "Email enviado com sucesso via Brevo. MessageId: {MessageId}",
                    messageId);

                return NotificationResult.ComSucesso(
                    $"Status: {response.StatusCode}",
                    messageId);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogWarning(
                    "Falha ao enviar email via Brevo. Status: {StatusCode}, Body: {Body}",
                    response.StatusCode,
                    body);

                return NotificationResult.ComFalha(
                    $"Falha no envio: {response.StatusCode}",
                    body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email via Brevo para {Destinatario}", destinatario);

            return NotificationResult.ComFalha(
                $"Exceção: {ex.Message}",
                ex.ToString());
        }
    }
}

// DTOs para API do Brevo
internal class BrevoSendEmailRequest
{
    [JsonPropertyName("sender")]
    public BrevoEmailAddress Sender { get; set; } = null!;

    [JsonPropertyName("to")]
    public BrevoEmailAddress[] To { get; set; } = Array.Empty<BrevoEmailAddress>();

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("htmlContent")]
    public string? HtmlContent { get; set; }

    [JsonPropertyName("textContent")]
    public string? TextContent { get; set; }

    [JsonPropertyName("replyTo")]
    public BrevoEmailAddress? ReplyTo { get; set; }
}

internal class BrevoEmailAddress
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal class BrevoSendEmailResponse
{
    [JsonPropertyName("messageId")]
    public string? MessageId { get; set; }
}
