using System.ComponentModel.DataAnnotations;
using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.RegraCobranca;

public class UpdateRegraCobrancaRequest
{
    [StringLength(200)]
    public string? Nome { get; set; }

    [StringLength(1000)]
    public string? Descricao { get; set; }

    public TipoMomento? TipoMomento { get; set; }

    [Range(1, 9999)]
    public int? ValorTempo { get; set; }

    public UnidadeTempo? UnidadeTempo { get; set; }

    public CanalNotificacao? CanalNotificacao { get; set; }

    public string? TemplateNotificacao { get; set; }

    [StringLength(150)]
    public string? SubjectEmail { get; set; }

    /// <summary>
    /// Lista de variáveis obrigatórias do sistema (campos na raiz do JSON do webhook)
    /// </summary>
    public List<string>? VariaveisObrigatoriasSistema { get; set; }

    public bool? Ativa { get; set; }
}
