namespace Cobrio.Application.DTOs.Assinatura;

public class AssinaturaResponse
{
    public Guid Id { get; set; }
    public Guid EmpresaClienteId { get; set; }
    public Guid AssinanteId { get; set; }
    public Guid PlanoOfertaId { get; set; }
    public string PlanoNome { get; set; } = string.Empty;
    public string AssinanteNome { get; set; } = string.Empty;
    public string AssinanteEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string TipoCiclo { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataProximaCobranca { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public DateTime? DataSuspensao { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public DateTime? TrialInicio { get; set; }
    public DateTime? TrialFim { get; set; }
    public bool EmTrial { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
