using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Cobrio.Infrastructure.Notifications;

public class TwilioWhatsAppProvider : IWhatsAppProvider
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioWhatsAppProvider> _logger;

    public CanalNotificacao TipoCanal => CanalNotificacao.WhatsApp;
    public string ProviderName => "Twilio";

    public TwilioWhatsAppProvider(
        IOptions<TwilioSettings> settings,
        ILogger<TwilioWhatsAppProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task<NotificationResult> EnviarAsync(
        string destinatario,
        string mensagem,
        string? assunto = null,
        CancellationToken cancellationToken = default)
    {
        return await EnviarWhatsAppAsync(destinatario, mensagem, cancellationToken);
    }

    public async Task<NotificationResult> EnviarWhatsAppAsync(
        string numeroDestino,
        string mensagem,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enviando WhatsApp via Twilio para {NumeroDestino}", numeroDestino);

            // Normalizar número (garantir formato whatsapp:+55XXXXXXXXXXX)
            var numeroFormatado = NormalizarNumeroWhatsApp(numeroDestino);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(numeroFormatado),
                from: new PhoneNumber($"whatsapp:{_settings.NumeroWhatsApp}"),
                body: mensagem);

            if (message.ErrorCode == null)
            {
                _logger.LogInformation(
                    "WhatsApp enviado com sucesso via Twilio. SID: {Sid}, Status: {Status}",
                    message.Sid,
                    message.Status);

                return NotificationResult.ComSucesso(
                    $"Status: {message.Status}, Price: {message.Price}",
                    message.Sid);
            }
            else
            {
                _logger.LogWarning(
                    "Falha ao enviar WhatsApp via Twilio. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    message.ErrorCode,
                    message.ErrorMessage);

                return NotificationResult.ComFalha(
                    $"Erro {message.ErrorCode}: {message.ErrorMessage}",
                    message.Status.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar WhatsApp via Twilio para {NumeroDestino}", numeroDestino);

            return NotificationResult.ComFalha(
                $"Exceção: {ex.Message}",
                ex.ToString());
        }
    }

    private string NormalizarNumeroWhatsApp(string numero)
    {
        // Se já estiver no formato whatsapp:, retornar
        if (numero.StartsWith("whatsapp:"))
        {
            return numero;
        }

        // Remove caracteres não numéricos
        var apenasNumeros = new string(numero.Where(char.IsDigit).ToArray());

        // Adicionar prefixo whatsapp: e código do Brasil se necessário
        if (apenasNumeros.Length == 11)
        {
            return $"whatsapp:+55{apenasNumeros}";
        }
        else if (apenasNumeros.Length == 13 && apenasNumeros.StartsWith("55"))
        {
            return $"whatsapp:+{apenasNumeros}";
        }
        else if (numero.StartsWith("+"))
        {
            return $"whatsapp:{numero}";
        }

        return $"whatsapp:+{apenasNumeros}";
    }
}

public class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string NumeroRemetente { get; set; } = string.Empty;
    public string NumeroWhatsApp { get; set; } = string.Empty;
}
