namespace Cobrio.Domain.Enums;

public enum StatusNotificacao
{
    // Status iniciais
    Pendente = 0,           // Aguardando envio
    Enviado = 1,            // Enviado ao provedor (Sent)

    // Status de sucesso
    Entregue = 2,           // Entregue com sucesso (Delivered)
    Aberto = 3,             // Email foi aberto (Opened)
    Clicado = 4,            // Link foi clicado (Clicked)

    // Status de erro temporário
    SoftBounce = 10,        // Erro temporário - caixa cheia, servidor indisponível
    Adiado = 11,            // Delivery adiado (Deferred)

    // Status de erro permanente
    HardBounce = 20,        // Erro permanente - email não existe
    EmailInvalido = 21,     // Formato de email inválido
    Bloqueado = 22,         // Bloqueado pelo provedor ou destinatário

    // Status de engajamento negativo
    Reclamacao = 30,        // Marcado como spam (Complaint)
    Descadastrado = 31,     // Unsubscribed

    // Status de erro do sistema
    ErroEnvio = 40,         // Erro ao tentar enviar

    // Manter compatibilidade com código antigo
    Sucesso = 2,            // Alias para Entregue
    Falha = 40              // Alias para ErroEnvio
}
