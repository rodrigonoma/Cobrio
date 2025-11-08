namespace Cobrio.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Envia um email com conteúdo HTML
    /// </summary>
    /// <param name="destinatario">Email do destinatário</param>
    /// <param name="assunto">Assunto do email</param>
    /// <param name="corpoHtml">Conteúdo HTML do email</param>
    /// <param name="remetenteEmail">Email do remetente (FROM)</param>
    /// <param name="remetenteNome">Nome do remetente (Display Name)</param>
    /// <param name="replyTo">Email para resposta (Reply-To)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o email foi enviado com sucesso</returns>
    Task<bool> EnviarEmailAsync(
        string destinatario,
        string assunto,
        string corpoHtml,
        string? remetenteEmail = null,
        string? remetenteNome = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default);
}
