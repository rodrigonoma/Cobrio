namespace Cobrio.Application.DTOs.Analytics;

public class DashboardAnalyticsResponse
{
    // KPIs Principais
    public decimal ValorRecuperadoMesAtual { get; set; }
    public decimal TaxaConversaoCobranca { get; set; }
    public int MensagensEnviadasTotal { get; set; }
    public int MensagensEnviadasUltimos7Dias { get; set; }
    public decimal TaxaEntregaMedia { get; set; }
    public decimal TaxaLeituraMedia { get; set; }
    public decimal TaxaCliqueMedia { get; set; }
    public double TempoMedioAtePagamentoDias { get; set; }
    public int MensagensComErro { get; set; }
    public decimal PercentualErro { get; set; }

    // Performance das Réguas
    public List<PerformanceReguaDto> PerformanceReguas { get; set; } = new();

    // Efetividade por Canal
    public List<EfetividadeCanalDto> EfetividadeCanais { get; set; } = new();

    // Análise Financeira
    public AnaliseFinanceiraDto AnaliseFinanceira { get; set; } = new();

    // Engajamento
    public EngajamentoDto Engajamento { get; set; } = new();

    // Status Operacional
    public StatusOperacionalDto StatusOperacional { get; set; } = new();

    // Timeline de Eventos
    public List<EventoTimelineDto> TimelineEventos { get; set; } = new();

    // Insights
    public List<InsightDto> Insights { get; set; } = new();
}

public class PerformanceReguaDto
{
    public Guid ReguaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Cobrança, Comunicação
    public string Canal { get; set; } = string.Empty;
    public decimal TaxaConversao { get; set; }
    public decimal ValorRecuperado { get; set; }
    public int MensagensEnviadas { get; set; }
    public int PagamentosGerados { get; set; }
}

public class EfetividadeCanalDto
{
    public string Canal { get; set; } = string.Empty; // Email, WhatsApp, SMS
    public int Entregues { get; set; }
    public int Lidas { get; set; }
    public int Respondidas { get; set; }
    public decimal TaxaConversao { get; set; }
    public decimal CustoMensagem { get; set; }
    public decimal PercentualEntrega { get; set; }
    public decimal PercentualLeitura { get; set; }
    public decimal PercentualResposta { get; set; }
}

public class AnaliseFinanceiraDto
{
    public decimal TotalRecuperado30Dias { get; set; }
    public decimal CustoTotalDisparos { get; set; }
    public decimal RoiMedio { get; set; }
    public decimal TicketMedioPago { get; set; }
    public List<ValorRecuperadoPorDiaDto> ValorPorDia { get; set; } = new();
    public List<ReceitaPorTipoReguaDto> ReceitaPorTipo { get; set; } = new();
}

public class ValorRecuperadoPorDiaDto
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
}

public class ReceitaPorTipoReguaDto
{
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Percentual { get; set; }
}

public class EngajamentoDto
{
    public string MelhorHorario { get; set; } = string.Empty;
    public string MelhorDiaSemana { get; set; } = string.Empty;
    public string CanalMaisEngajamento { get; set; } = string.Empty;
    public List<HeatmapHorarioDto> HeatmapHorarios { get; set; } = new();
    public List<ClienteEngajadoDto> ClientesMaisEngajados { get; set; } = new();
}

public class HeatmapHorarioDto
{
    public int DiaSemana { get; set; } // 0 = Domingo, 6 = Sábado
    public int Hora { get; set; }
    public int QuantidadeEngajamentos { get; set; }
    public decimal TaxaEngajamento { get; set; }
}

public class ClienteEngajadoDto
{
    public string Nome { get; set; } = string.Empty;
    public int Aberturas { get; set; }
    public int Respostas { get; set; }
    public int Cliques { get; set; }
}

public class StatusOperacionalDto
{
    public StatusFilaDto FilaEnvio { get; set; } = new();
    public StatusWebhookDto Webhook { get; set; } = new();
    public List<StatusProvedorDto> Provedores { get; set; } = new();
}

public class StatusFilaDto
{
    public string Status { get; set; } = string.Empty; // OK, Alerta, Erro
    public DateTime? UltimaExecucao { get; set; }
    public int MensagensNaFila { get; set; }
}

public class StatusWebhookDto
{
    public string Status { get; set; } = string.Empty; // Ativo, Inativo, Erro
    public DateTime? UltimaAtualizacao { get; set; }
}

public class StatusProvedorDto
{
    public string Nome { get; set; } = string.Empty; // Brevo, Twilio
    public string Status { get; set; } = string.Empty; // OK, Alerta, Erro
    public decimal PercentualUtilizado { get; set; }
    public int LimiteTotal { get; set; }
    public int LimiteUtilizado { get; set; }
}

public class EventoTimelineDto
{
    public DateTime DataHora { get; set; }
    public string Tipo { get; set; } = string.Empty; // Envio, Entrega, Leitura, Pagamento, Erro
    public string Severidade { get; set; } = string.Empty; // Success, Info, Warning, Danger
    public string Descricao { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public decimal? Valor { get; set; }
}

public class InsightDto
{
    public string Tipo { get; set; } = string.Empty; // Insight, Recomendacao, Alerta
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty; // blue, green, yellow, red
}
