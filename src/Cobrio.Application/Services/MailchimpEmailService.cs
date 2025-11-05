using Cobrio.Application.Configuration;
using Cobrio.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cobrio.Application.Services;

public class BrevoEmailService : IEmailService
{
    private readonly BrevoSettings _settings;
    private readonly ILogger<BrevoEmailService> _logger;
    private readonly HttpClient _httpClient;

    public BrevoEmailService(
        IOptions<BrevoSettings> settings,
        ILogger<BrevoEmailService> logger,
        HttpClient httpClient)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.brevo.com/v3/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
    }

    public async Task<bool> EnviarEmailAsync(
        string destinatario,
        string assunto,
        string corpoHtml,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enviando email via Brevo para {Destinatario} com assunto '{Assunto}'",
                destinatario, assunto);

            var payload = new
            {
                sender = new
                {
                    name = _settings.FromName,
                    email = _settings.FromEmail
                },
                to = new[]
                {
                    new
                    {
                        email = destinatario
                    }
                },
                subject = assunto,
                htmlContent = corpoHtml
            };

            var response = await _httpClient.PostAsJsonAsync("smtp/email", payload, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BrevoResponse>(content);

                _logger.LogInformation("Email enviado com sucesso para {Destinatario}. MessageId: {MessageId}",
                    destinatario, result?.messageId);
                return true;
            }
            else
            {
                _logger.LogError("Erro ao enviar email para {Destinatario}. Status: {Status}, Response: {Response}",
                    destinatario, response.StatusCode, content);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar email via Brevo para {Destinatario}",
                destinatario);
            return false;
        }
    }

    private class BrevoResponse
    {
        public string messageId { get; set; } = string.Empty;
    }
}
