using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.RegraCobranca;

public class RegraCobrancaResponse
{
    public Guid Id { get; set; }
    public Guid EmpresaClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativa { get; set; }
    public bool EhPadrao { get; set; }
    public TipoMomento TipoMomento { get; set; }
    public int ValorTempo { get; set; }
    public UnidadeTempo UnidadeTempo { get; set; }
    public CanalNotificacao CanalNotificacao { get; set; }
    public string TemplateNotificacao { get; set; } = string.Empty;
    public string? SubjectEmail { get; set; }
    public List<string> VariaveisObrigatorias { get; set; } = new();
    public string? VariaveisObrigatoriasSistema { get; set; }
    public string TokenWebhook { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
