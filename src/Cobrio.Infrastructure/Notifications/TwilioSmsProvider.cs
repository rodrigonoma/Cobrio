using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Cobrio.Infrastructure.Notifications;

public class TwilioSmsProvider : ISmsProvider
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioSmsProvider> _logger;

    public CanalNotificacao TipoCanal => CanalNotificacao.SMS;
    public string ProviderName => "Twilio";

    public TwilioSmsProvider(
        IOptions<TwilioSettings> settings,
        ILogger<TwilioSmsProvider> logger)
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
        return await EnviarSmsAsync(destinatario, mensagem, cancellationToken);
    }

    public async Task<NotificationResult> EnviarSmsAsync(
        string numeroDestino,
        string mensagem,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enviando SMS via Twilio para {NumeroDestino}", numeroDestino);

            // Normalizar número (garantir formato +55XXXXXXXXXXX)
            var numeroFormatado = NormalizarNumero(numeroDestino);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(numeroFormatado),
                from: new PhoneNumber(_settings.NumeroRemetente),
                body: mensagem);

            if (message.ErrorCode == null)
            {
                _logger.LogInformation(
                    "SMS enviado com sucesso via Twilio. SID: {Sid}, Status: {Status}",
                    message.Sid,
                    message.Status);

                return NotificationResult.ComSucesso(
                    $"Status: {message.Status}, Price: {message.Price}",
                    message.Sid);
            }
            else
            {
                _logger.LogWarning(
                    "Falha ao enviar SMS via Twilio. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    message.ErrorCode,
                    message.ErrorMessage);

                return NotificationResult.ComFalha(
                    $"Erro {message.ErrorCode}: {message.ErrorMessage}",
                    message.Status.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar SMS via Twilio para {NumeroDestino}", numeroDestino);

            return NotificationResult.ComFalha(
                $"Exceção: {ex.Message}",
                ex.ToString());
        }
    }

    private string NormalizarNumero(string numero)
    {
        // Remove caracteres não numéricos
        var apenasNumeros = new string(numero.Where(char.IsDigit).ToArray());

        // Se não começar com +, adicionar código do Brasil (+55)
        if (!numero.StartsWith("+"))
        {
            // Se tiver 11 dígitos (celular com DDD), adicionar +55
            if (apenasNumeros.Length == 11)
            {
                return $"+55{apenasNumeros}";
            }
            // Se já tiver código do país (55 + 11 dígitos)
            else if (apenasNumeros.Length == 13 && apenasNumeros.StartsWith("55"))
            {
                return $"+{apenasNumeros}";
            }
        }

        return numero;
    }
}
