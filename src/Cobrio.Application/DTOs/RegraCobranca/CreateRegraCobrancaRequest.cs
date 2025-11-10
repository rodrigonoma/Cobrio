using System.ComponentModel.DataAnnotations;
using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.RegraCobranca;

public class CreateRegraCobrancaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Tipo de momento é obrigatório")]
    public TipoMomento TipoMomento { get; set; }

    [Required(ErrorMessage = "Valor do tempo é obrigatório")]
    [Range(1, 9999, ErrorMessage = "Valor do tempo deve estar entre 1 e 9999")]
    public int ValorTempo { get; set; }

    [Required(ErrorMessage = "Unidade de tempo é obrigatória")]
    public UnidadeTempo UnidadeTempo { get; set; }

    [Required(ErrorMessage = "Canal de notificação é obrigatório")]
    public CanalNotificacao CanalNotificacao { get; set; }

    [Required(ErrorMessage = "Template de notificação é obrigatório")]
    public string TemplateNotificacao { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Assunto do email deve ter no máximo 150 caracteres")]
    public string? SubjectEmail { get; set; }

    /// <summary>
    /// Lista de variáveis obrigatórias do sistema (campos na raiz do JSON do webhook)
    /// </summary>
    public List<string>? VariaveisObrigatoriasSistema { get; set; }
}
