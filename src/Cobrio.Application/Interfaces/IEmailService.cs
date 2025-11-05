namespace Cobrio.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Envia um email com conteúdo HTML
    /// </summary>
    /// <param name="destinatario">Email do destinatário</param>
    /// <param name="assunto">Assunto do email</param>
    /// <param name="corpoHtml">Conteúdo HTML do email</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o email foi enviado com sucesso</returns>
    Task<bool> EnviarEmailAsync(
        string destinatario,
        string assunto,
        string corpoHtml,
        CancellationToken cancellationToken = default);
}
