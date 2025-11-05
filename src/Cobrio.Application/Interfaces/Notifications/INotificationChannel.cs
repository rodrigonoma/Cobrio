using Cobrio.Domain.Enums;

namespace Cobrio.Application.Interfaces.Notifications;

/// <summary>
/// Interface base para todos os canais de notificação
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Tipo de canal que este provider implementa
    /// </summary>
    CanalNotificacao TipoCanal { get; }

    /// <summary>
    /// Envia uma notificação
    /// </summary>
    /// <param name="destinatario">Email, telefone, etc.</param>
    /// <param name="mensagem">Mensagem a ser enviada</param>
    /// <param name="assunto">Assunto (apenas para email)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado do envio com sucesso e resposta do provedor</returns>
    Task<NotificationResult> EnviarAsync(
        string destinatario,
        string mensagem,
        string? assunto = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de uma tentativa de envio de notificação
/// </summary>
public class NotificationResult
{
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
    public string? RespostaProvedor { get; set; }
    public string? IdRastreamento { get; set; }

    public static NotificationResult ComSucesso(string? respostaProvedor = null, string? idRastreamento = null)
    {
        return new NotificationResult
        {
            Sucesso = true,
            RespostaProvedor = respostaProvedor,
            IdRastreamento = idRastreamento
        };
    }

    public static NotificationResult ComFalha(string mensagemErro, string? respostaProvedor = null)
    {
        return new NotificationResult
        {
            Sucesso = false,
            MensagemErro = mensagemErro,
            RespostaProvedor = respostaProvedor
        };
    }
}
