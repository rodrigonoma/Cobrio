using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

/// <summary>
/// Registra cada mudança de status de uma notificação (timeline completa)
/// </summary>
public class HistoricoStatusNotificacao : BaseEntity
{
    public Guid HistoricoNotificacaoId { get; private set; }
    public StatusNotificacao StatusAnterior { get; private set; }
    public StatusNotificacao StatusNovo { get; private set; }
    public DateTime DataMudanca { get; private set; }
    public string? Detalhes { get; private set; }
    public string? IpOrigem { get; private set; }
    public string? UserAgent { get; private set; }

    // Navegação
    public HistoricoNotificacao HistoricoNotificacao { get; private set; } = null!;

    // Construtor para EF Core
    private HistoricoStatusNotificacao() { }

    public HistoricoStatusNotificacao(
        Guid historicoNotificacaoId,
        StatusNotificacao statusAnterior,
        StatusNotificacao statusNovo,
        string? detalhes = null,
        string? ipOrigem = null,
        string? userAgent = null)
    {
        if (historicoNotificacaoId == Guid.Empty)
            throw new ArgumentException("HistoricoNotificacaoId inválido", nameof(historicoNotificacaoId));

        HistoricoNotificacaoId = historicoNotificacaoId;
        StatusAnterior = statusAnterior;
        StatusNovo = statusNovo;
        DataMudanca = DateTime.UtcNow;
        Detalhes = detalhes;
        IpOrigem = ipOrigem;
        UserAgent = userAgent;
    }

    public static HistoricoStatusNotificacao Criar(
        Guid historicoNotificacaoId,
        StatusNotificacao statusAnterior,
        StatusNotificacao statusNovo,
        string? detalhes = null,
        string? ipOrigem = null,
        string? userAgent = null)
    {
        return new HistoricoStatusNotificacao(
            historicoNotificacaoId,
            statusAnterior,
            statusNovo,
            detalhes,
            ipOrigem,
            userAgent);
    }
}
