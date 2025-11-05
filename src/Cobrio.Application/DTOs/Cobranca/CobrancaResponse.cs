using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.Cobranca;

public class CobrancaResponse
{
    public Guid Id { get; set; }
    public Guid RegraCobrancaId { get; set; }
    public string RegraCobrancaNome { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime DataDisparo { get; set; }
    public StatusCobranca Status { get; set; }
    public int TentativasEnvio { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public string? MensagemErro { get; set; }
    public DateTime CriadoEm { get; set; }
}
