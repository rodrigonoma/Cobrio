using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.Relatorios;

// ============================================================================
// RELATÓRIOS OPERACIONAIS
// ============================================================================

public class DashboardOperacionalResponse
{
    public int TotalCobrancas { get; set; }
    public int CobrancasProcessadas { get; set; }
    public int CobrancasFalhas { get; set; }
    public int CobrancasPendentes { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorProcessado { get; set; }
    public decimal MediaTentativas { get; set; }
    public int CobrancasRetentadas { get; set; }
    public decimal VariacaoProcessadas { get; set; }
    public decimal TaxaSucesso { get; set; }
}

public class ExecucaoReguaResponse
{
    public DateTime Data { get; set; }
    public CanalNotificacao Canal { get; set; }
    public string NomeRegra { get; set; } = string.Empty;
    public Guid RegraId { get; set; }
    public int TotalEnvios { get; set; }
    public int Sucessos { get; set; }
    public int Falhas { get; set; }
    public decimal TaxaSucesso { get; set; }
    public int CobrancasProcessadas { get; set; }
    public int CobrancasFalhas { get; set; }
}

public class EntregasFalhasResponse
{
    public List<FalhasPorTipoResponse> FalhasPorTipo { get; set; } = new();
    public List<PendenciasPorTempoResponse> PendenciasPorTempo { get; set; } = new();
}

public class FalhasPorTipoResponse
{
    public DateTime Data { get; set; }
    public string TipoErro { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public List<Guid> RegrasAfetadas { get; set; } = new();
}

public class PendenciasPorTempoResponse
{
    public string FaixaTempo { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
}

public class CobrancasRecebimentosResponse
{
    public DateTime Data { get; set; }
    public int TotalCobrancasEnviadas { get; set; }
    public decimal ValorCobrado { get; set; }
    public int TotalFaturasGeradas { get; set; }
    public int FaturasPagas { get; set; }
    public decimal ValorRecebido { get; set; }
    public decimal TaxaRecuperacao { get; set; }
    public decimal MediaHorasAtePagamento { get; set; }
    public int FaturasVencidas { get; set; }
}

public class ValoresPorReguaResponse
{
    public Guid RegraId { get; set; }
    public string NomeRegra { get; set; } = string.Empty;
    public CanalNotificacao Canal { get; set; }
    public string DescricaoTiming { get; set; } = string.Empty;
    public int TotalCobrancas { get; set; }
    public decimal ValorCobrado { get; set; }
    public int FaturasPagas { get; set; }
    public decimal ValorRecuperado { get; set; }
    public decimal TaxaRecuperacao { get; set; }
    public decimal CustoEstimado { get; set; }
    public decimal ROI { get; set; }
}

public class PagamentosPorAtrasoResponse
{
    public string FaixaAtraso { get; set; } = string.Empty;
    public int QuantidadeFaturas { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal MediaCobrancasEnviadas { get; set; }
    public decimal MediaDiasAteRecuperacao { get; set; }
}

// ============================================================================
// RELATÓRIOS GERENCIAIS
// ============================================================================

public class ConversaoPorCanalResponse
{
    public string NomeRegra { get; set; } = string.Empty;
    public CanalNotificacao Canal { get; set; }
    public FunilConversaoDto Funil { get; set; } = new();
    public decimal TaxaProcessamento { get; set; }
    public decimal TaxaConversaoFinal { get; set; }
    public decimal ValorRecuperado { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal MediaHorasConversao { get; set; }
}

public class FunilConversaoDto
{
    public int TotalEnvios { get; set; }
    public int TotalCobrancas { get; set; }
    public int CobrancasProcessadas { get; set; }
    public int FaturasPagas { get; set; }
}

public class ROIReguasResponse
{
    public string Periodo { get; set; } = string.Empty;
    public Guid RegraId { get; set; }
    public string NomeRegra { get; set; } = string.Empty;
    public CanalNotificacao Canal { get; set; }
    public int TotalEnvios { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal ReceitaRecuperada { get; set; }
    public decimal ROI { get; set; }
    public decimal LucroLiquido { get; set; }
}

public class EvolucaoMensalResponse
{
    public string Periodo { get; set; } = string.Empty;
    public int TotalCobrancas { get; set; }
    public int CobrancasProcessadas { get; set; }
    public decimal ValorCobrado { get; set; }
    public int FaturasPagas { get; set; }
    public decimal ValorRecebido { get; set; }
    public decimal TaxaSucesso { get; set; }
    public decimal TaxaConversao { get; set; }
    public int FaturasInadimplentes { get; set; }
    public BreakdownCanalDto BreakdownCanal { get; set; } = new();
}

public class BreakdownCanalDto
{
    public int EnviosEmail { get; set; }
    public int EnviosSMS { get; set; }
    public int EnviosWhatsApp { get; set; }
}

public class MelhorHorarioEnvioResponse
{
    public List<PerformancePorDiaResponse> PorDiaSemana { get; set; } = new();
    public List<PerformancePorHoraResponse> PorHoraDia { get; set; } = new();
    public RecomendacaoEnvioDto Recomendacao { get; set; } = new();
}

public class PerformancePorDiaResponse
{
    public int DiaSemana { get; set; }
    public string NomeDia { get; set; } = string.Empty;
    public int TotalCobrancas { get; set; }
    public int FaturasPagas { get; set; }
    public decimal TaxaConversao { get; set; }
    public decimal MediaHorasConversao { get; set; }
}

public class PerformancePorHoraResponse
{
    public int Hora { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public int TotalCobrancas { get; set; }
    public int FaturasPagas { get; set; }
    public decimal TaxaConversao { get; set; }
    public decimal MediaHorasConversao { get; set; }
}

public class RecomendacaoEnvioDto
{
    public string MelhorDia { get; set; } = string.Empty;
    public string MelhorPeriodo { get; set; } = string.Empty;
    public decimal TaxaConversaoEsperada { get; set; }
}

public class ReducaoInadimplenciaResponse
{
    public MetricasComReguaDto ComRegua { get; set; } = new();
    public MetricasSemReguaDto SemRegua { get; set; } = new();
    public ImpactoReguaDto Impacto { get; set; } = new();
}

public class MetricasComReguaDto
{
    public int TotalFaturas { get; set; }
    public int FaturasPagas { get; set; }
    public decimal MediaDiasAtraso { get; set; }
    public decimal TaxaPagamento { get; set; }
}

public class MetricasSemReguaDto
{
    public int TotalFaturas { get; set; }
    public int FaturasPagas { get; set; }
    public decimal MediaDiasAtraso { get; set; }
    public decimal TaxaPagamento { get; set; }
}

public class ImpactoReguaDto
{
    public decimal PontoPercentualMelhoria { get; set; }
    public decimal ReducaoDiasAtraso { get; set; }
    public string InterpretacaoImpacto { get; set; } = string.Empty;
}

// ============================================================================
// RELATÓRIOS HÍBRIDOS (OMNICHANNEL)
// ============================================================================

public class TempoEnvioPagamentoResponse
{
    public string NomeRegua { get; set; } = string.Empty;
    public List<CanalNotificacao> CanaisUtilizados { get; set; } = new();
    public int QuantidadeCanais { get; set; }
    public decimal MediaHorasPrimeiroEnvio { get; set; }
    public decimal MediaHorasUltimoEnvio { get; set; }
    public int TotalFaturasPagas { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal MediaEnviosAtePagamento { get; set; }
    public bool EhOmnichannel => QuantidadeCanais > 1;
}

public class ComparativoOmnichannelResponse
{
    public string TipoEstrategia { get; set; } = string.Empty;
    public List<CanalNotificacao> CombinacaoCanais { get; set; } = new();
    public int TotalCobrancas { get; set; }
    public int FaturasPagas { get; set; }
    public decimal TaxaConversao { get; set; }
    public decimal ValorRecuperado { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal MediaHorasConversao { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal ROI { get; set; }
}

// ============================================================================
// FILTROS
// ============================================================================

public class FiltrosRelatoriosAvancados
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<Guid>? RegraCobrancaIds { get; set; }
    public List<CanalNotificacao>? CanaisNotificacao { get; set; }
    public List<StatusCobranca>? StatusCobranca { get; set; }
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }
    public string? AgruparPor { get; set; } // dia, semana, mes
    public bool CompararComPeriodoAnterior { get; set; }
}

// ============================================================================
// PAGINAÇÃO
// ============================================================================

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int ItensPorPagina { get; set; }
    public int TotalPaginas { get; set; }
}
