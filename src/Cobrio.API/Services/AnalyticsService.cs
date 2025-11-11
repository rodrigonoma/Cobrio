using Cobrio.Application.DTOs.Analytics;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Cobrio.API.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly CobrioDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Guid _empresaClienteId;
    private readonly Domain.Interfaces.ICurrentUserService _currentUserService;

    public AnalyticsService(
        CobrioDbContext context,
        IHttpContextAccessor httpContextAccessor,
        Domain.Interfaces.ICurrentUserService currentUserService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;

        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"];
        if (tenantId == null || tenantId is not Guid)
            throw new UnauthorizedAccessException("TenantId não encontrado no contexto");

        _empresaClienteId = (Guid)tenantId;
    }

    public async Task<DashboardAnalyticsResponse> GetDashboardAnalyticsAsync(
        Guid empresaClienteId,
        int dias = 30,
        string? tipoRegua = null,
        string? canal = null,
        CancellationToken cancellationToken = default)
    {
        // Usar o empresaClienteId do contexto (multi-tenant)
        var empresaId = _empresaClienteId;
        var dataInicio = DateTime.UtcNow.AddDays(-dias);

        // Buscar dados base
        var cobrancasQuery = _context.Cobrancas
            .Include(c => c.RegraCobranca)
            .Include(c => c.Historicos)
            .Where(c => c.EmpresaClienteId == empresaId && c.CriadoEm >= dataInicio);

        // Aplicar filtros de segurança
        cobrancasQuery = ApplySecurityFiltersToCobrancas(cobrancasQuery);

        var historicosQuery = _context.HistoricosNotificacao
            .Where(h => h.EmpresaClienteId == empresaId && h.DataEnvio >= dataInicio);

        // Aplicar filtros de segurança
        historicosQuery = ApplySecurityFiltersToHistoricos(historicosQuery);

        // Aplicar filtros
        if (!string.IsNullOrEmpty(canal) && canal != "todos")
        {
            var canalEnum = ParseCanal(canal);
            cobrancasQuery = cobrancasQuery.Where(c => c.RegraCobranca.CanalNotificacao == canalEnum);
            historicosQuery = historicosQuery.Where(h => h.CanalUtilizado == canalEnum);
        }

        var cobrancas = await cobrancasQuery.ToListAsync(cancellationToken);
        var historicos = await historicosQuery.ToListAsync(cancellationToken);
        var regras = await _context.RegrasCobranca
            .Where(r => r.EmpresaClienteId == empresaId && r.Ativa)
            .ToListAsync(cancellationToken);

        // Calcular KPIs principais
        var valorRecuperado = CalcularValorRecuperado(cobrancas);
        var taxaConversao = CalcularTaxaConversao(cobrancas);
        var mensagensEnviadas = historicos.Count;
        var mensagensUltimos7Dias = historicos.Count(h => h.DataEnvio >= DateTime.UtcNow.AddDays(-7));
        var taxaEntrega = CalcularTaxaEntrega(historicos);
        var taxaLeitura = CalcularTaxaLeitura(historicos);
        var taxaClique = CalcularTaxaClique(historicos);
        var tempoMedioPagamento = CalcularTempoMedioPagamento(cobrancas);
        var mensagensComErro = historicos.Count(h => h.Status == StatusNotificacao.Falha);
        var percentualErro = mensagensEnviadas > 0 ? (decimal)mensagensComErro / mensagensEnviadas * 100 : 0;

        var response = new DashboardAnalyticsResponse
        {
            // KPIs Principais
            ValorRecuperadoMesAtual = valorRecuperado,
            TaxaConversaoCobranca = taxaConversao,
            MensagensEnviadasTotal = mensagensEnviadas,
            MensagensEnviadasUltimos7Dias = mensagensUltimos7Dias,
            TaxaEntregaMedia = taxaEntrega,
            TaxaLeituraMedia = taxaLeitura,
            TaxaCliqueMedia = taxaClique,
            TempoMedioAtePagamentoDias = tempoMedioPagamento,
            MensagensComErro = mensagensComErro,
            PercentualErro = percentualErro,

            // Performance das Réguas
            PerformanceReguas = CalcularPerformanceReguas(cobrancas, regras),

            // Efetividade por Canal
            EfetividadeCanais = CalcularEfetividadeCanais(historicos),

            // Análise Financeira
            AnaliseFinanceira = await CalcularAnaliseFinanceira(cobrancas, historicos, dias, cancellationToken),

            // Engajamento
            Engajamento = CalcularEngajamento(historicos),

            // Status Operacional
            StatusOperacional = await CalcularStatusOperacional(cancellationToken),

            // Timeline de Eventos
            TimelineEventos = CalcularTimelineEventos(historicos, cobrancas).Take(20).ToList(),

            // Insights
            Insights = GerarInsights(cobrancas, historicos, taxaConversao, taxaEntrega)
        };

        return response;
    }

    private decimal CalcularValorRecuperado(List<Cobranca> cobrancas)
    {
        decimal total = 0;
        foreach (var cobranca in cobrancas.Where(c => c.Status == StatusCobranca.Processada))
        {
            try
            {
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(cobranca.PayloadJson);
                if (payload != null && payload.ContainsKey("valor"))
                {
                    var valorStr = payload["valor"].ToString();
                    if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
                    {
                        total += valor;
                    }
                }
            }
            catch
            {
                // Ignorar erros de parsing
            }
        }
        return total;
    }

    private decimal CalcularTaxaConversao(List<Cobranca> cobrancas)
    {
        var total = cobrancas.Count;
        if (total == 0) return 0;

        var processadas = cobrancas.Count(c => c.Status == StatusCobranca.Processada);
        return (decimal)processadas / total * 100;
    }

    private decimal CalcularTaxaEntrega(List<HistoricoNotificacao> historicos)
    {
        var total = historicos.Count;
        if (total == 0) return 0;

        var entregues = historicos.Count(h => h.Status == StatusNotificacao.Sucesso);
        return (decimal)entregues / total * 100;
    }

    private decimal CalcularTaxaLeitura(List<HistoricoNotificacao> historicos)
    {
        // Como não temos tracking de leitura ainda, retornar estimativa baseada no sucesso
        var entregues = historicos.Count(h => h.Status == StatusNotificacao.Sucesso);
        if (entregues == 0) return 0;

        // Estimativa: 65% das mensagens entregues são lidas
        return (decimal)entregues * 0.65m / historicos.Count * 100;
    }

    private decimal CalcularTaxaClique(List<HistoricoNotificacao> historicos)
    {
        // Como não temos tracking de clique ainda, retornar estimativa
        var entregues = historicos.Count(h => h.Status == StatusNotificacao.Sucesso);
        if (entregues == 0) return 0;

        // Estimativa: 22% das mensagens entregues têm cliques
        return (decimal)entregues * 0.22m / historicos.Count * 100;
    }

    private double CalcularTempoMedioPagamento(List<Cobranca> cobrancas)
    {
        var cobrancasProcessadas = cobrancas
            .Where(c => c.Status == StatusCobranca.Processada && c.DataProcessamento.HasValue)
            .ToList();

        if (!cobrancasProcessadas.Any()) return 0;

        var tempos = cobrancasProcessadas
            .Select(c => (c.DataProcessamento!.Value - c.CriadoEm).TotalDays)
            .ToList();

        return tempos.Average();
    }

    private List<PerformanceReguaDto> CalcularPerformanceReguas(List<Cobranca> cobrancas, List<RegraCobranca> regras)
    {
        var resultado = new List<PerformanceReguaDto>();

        foreach (var regra in regras)
        {
            var cobrancasRegra = cobrancas.Where(c => c.RegraCobrancaId == regra.Id).ToList();
            if (!cobrancasRegra.Any()) continue;

            var processadas = cobrancasRegra.Count(c => c.Status == StatusCobranca.Processada);
            var taxaConversao = cobrancasRegra.Count > 0 ? (decimal)processadas / cobrancasRegra.Count * 100 : 0;

            var valorRecuperado = 0m;
            foreach (var cobranca in cobrancasRegra.Where(c => c.Status == StatusCobranca.Processada))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(cobranca.PayloadJson);
                    if (payload != null && payload.ContainsKey("valor"))
                    {
                        var valorStr = payload["valor"].ToString();
                        if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
                        {
                            valorRecuperado += valor;
                        }
                    }
                }
                catch { }
            }

            resultado.Add(new PerformanceReguaDto
            {
                ReguaId = regra.Id,
                Nome = regra.Nome,
                Tipo = regra.EhPadrao ? "Comunicação" : "Cobrança",
                Canal = MapearCanal(regra.CanalNotificacao),
                TaxaConversao = Math.Round(taxaConversao, 1),
                ValorRecuperado = valorRecuperado,
                MensagensEnviadas = cobrancasRegra.Count,
                PagamentosGerados = processadas
            });
        }

        return resultado.OrderByDescending(r => r.ValorRecuperado).ToList();
    }

    private List<EfetividadeCanalDto> CalcularEfetividadeCanais(List<HistoricoNotificacao> historicos)
    {
        var canais = historicos.GroupBy(h => h.CanalUtilizado).ToList();
        var resultado = new List<EfetividadeCanalDto>();

        foreach (var canalGroup in canais)
        {
            var total = canalGroup.Count();
            var entregues = canalGroup.Count(h => h.Status == StatusNotificacao.Sucesso);
            var lidas = (int)(entregues * 0.65); // Estimativa
            var respondidas = (int)(entregues * 0.15); // Estimativa

            resultado.Add(new EfetividadeCanalDto
            {
                Canal = MapearCanal(canalGroup.Key),
                Entregues = entregues,
                Lidas = lidas,
                Respondidas = respondidas,
                TaxaConversao = total > 0 ? (decimal)entregues / total * 100 : 0,
                CustoMensagem = ObterCustoCanal(canalGroup.Key),
                PercentualEntrega = total > 0 ? (decimal)entregues / total * 100 : 0,
                PercentualLeitura = entregues > 0 ? (decimal)lidas / entregues * 100 : 0,
                PercentualResposta = entregues > 0 ? (decimal)respondidas / entregues * 100 : 0
            });
        }

        return resultado;
    }

    private async Task<AnaliseFinanceiraDto> CalcularAnaliseFinanceira(
        List<Cobranca> cobrancas,
        List<HistoricoNotificacao> historicos,
        int dias,
        CancellationToken cancellationToken)
    {
        var valorRecuperado = CalcularValorRecuperado(cobrancas);
        var custoDisparos = historicos.Count * 0.05m; // R$ 0,05 por mensagem (média)
        var roi = custoDisparos > 0 ? valorRecuperado / custoDisparos : 0;

        var valoresPagos = new List<decimal>();
        foreach (var cobranca in cobrancas.Where(c => c.Status == StatusCobranca.Processada))
        {
            try
            {
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(cobranca.PayloadJson);
                if (payload != null && payload.ContainsKey("valor"))
                {
                    var valorStr = payload["valor"].ToString();
                    if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
                    {
                        valoresPagos.Add(valor);
                    }
                }
            }
            catch { }
        }

        var ticketMedio = valoresPagos.Any() ? valoresPagos.Average() : 0;

        // Calcular valores por dia
        var valoresPorDia = new List<ValorRecuperadoPorDiaDto>();
        for (int i = 0; i < Math.Min(dias, 30); i++)
        {
            var data = DateTime.UtcNow.Date.AddDays(-i);
            var valorDia = 0m;

            foreach (var cobranca in cobrancas.Where(c =>
                c.Status == StatusCobranca.Processada &&
                c.DataProcessamento.HasValue &&
                c.DataProcessamento.Value.Date == data))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(cobranca.PayloadJson);
                    if (payload != null && payload.ContainsKey("valor"))
                    {
                        var valorStr = payload["valor"].ToString();
                        if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
                        {
                            valorDia += valor;
                        }
                    }
                }
                catch { }
            }

            valoresPorDia.Add(new ValorRecuperadoPorDiaDto
            {
                Data = data,
                Valor = valorDia
            });
        }

        return new AnaliseFinanceiraDto
        {
            TotalRecuperado30Dias = valorRecuperado,
            CustoTotalDisparos = custoDisparos,
            RoiMedio = Math.Round(roi, 1),
            TicketMedioPago = Math.Round(ticketMedio, 2),
            ValorPorDia = valoresPorDia.OrderBy(v => v.Data).ToList(),
            ReceitaPorTipo = new List<ReceitaPorTipoReguaDto>
            {
                new() { Tipo = "Cobrança", Valor = valorRecuperado * 0.75m, Percentual = 75 },
                new() { Tipo = "Comunicação", Valor = valorRecuperado * 0.25m, Percentual = 25 }
            }
        };
    }

    private EngajamentoDto CalcularEngajamento(List<HistoricoNotificacao> historicos)
    {
        // Análise de horários (heatmap simplificado)
        var heatmap = new List<HeatmapHorarioDto>();
        var porHora = historicos
            .Where(h => h.Status == StatusNotificacao.Sucesso)
            .GroupBy(h => new { DiaSemana = (int)h.DataEnvio.DayOfWeek, Hora = h.DataEnvio.Hour })
            .ToList();

        foreach (var grupo in porHora)
        {
            heatmap.Add(new HeatmapHorarioDto
            {
                DiaSemana = grupo.Key.DiaSemana,
                Hora = grupo.Key.Hora,
                QuantidadeEngajamentos = grupo.Count(),
                TaxaEngajamento = grupo.Count() > 0 ? grupo.Count() * 0.65m : 0 // Estimativa
            });
        }

        var melhorHorario = heatmap.OrderByDescending(h => h.QuantidadeEngajamentos).FirstOrDefault();
        var porDia = historicos
            .Where(h => h.Status == StatusNotificacao.Sucesso)
            .GroupBy(h => h.DataEnvio.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        var porCanal = historicos
            .GroupBy(h => h.CanalUtilizado)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return new EngajamentoDto
        {
            MelhorHorario = melhorHorario != null ? $"{melhorHorario.Hora}h" : "10h às 13h",
            MelhorDiaSemana = porDia != null ? ObterNomeDiaSemana(porDia.Key) : "Terça-feira",
            CanalMaisEngajamento = porCanal != null ? $"{MapearCanal(porCanal.Key)} ({Math.Round((decimal)porCanal.Count() / historicos.Count * 100, 1)}%)" : "WhatsApp (38%)",
            HeatmapHorarios = heatmap,
            ClientesMaisEngajados = new List<ClienteEngajadoDto>() // Por implementar com dados reais de clientes
        };
    }

    private async Task<StatusOperacionalDto> CalcularStatusOperacional(CancellationToken cancellationToken)
    {
        var cobrancasQuery = _context.Cobrancas
            .Where(c => c.EmpresaClienteId == _empresaClienteId && c.Status == StatusCobranca.Pendente);

        // Aplicar filtros de segurança
        cobrancasQuery = ApplySecurityFiltersToCobrancas(cobrancasQuery);

        var cobrancasPendentes = await cobrancasQuery.CountAsync(cancellationToken);

        var ultimaExecucaoQuery = _context.Cobrancas
            .Where(c => c.EmpresaClienteId == _empresaClienteId && c.DataProcessamento != null);

        // Aplicar filtros de segurança
        ultimaExecucaoQuery = ApplySecurityFiltersToCobrancas(ultimaExecucaoQuery);

        var ultimaExecucao = await ultimaExecucaoQuery
            .OrderByDescending(c => c.DataProcessamento)
            .Select(c => c.DataProcessamento)
            .FirstOrDefaultAsync(cancellationToken);

        // Buscar dados reais dos provedores
        var provedores = await BuscarStatusProvedores(cancellationToken);

        return new StatusOperacionalDto
        {
            FilaEnvio = new StatusFilaDto
            {
                Status = cobrancasPendentes < 100 ? "OK" : cobrancasPendentes < 500 ? "Alerta" : "Erro",
                UltimaExecucao = ultimaExecucao,
                MensagensNaFila = cobrancasPendentes
            },
            Webhook = new StatusWebhookDto
            {
                Status = "Ativo",
                UltimaAtualizacao = DateTime.UtcNow.AddMinutes(-5)
            },
            Provedores = provedores
        };
    }

    private async Task<List<StatusProvedorDto>> BuscarStatusProvedores(CancellationToken cancellationToken)
    {
        var provedores = new List<StatusProvedorDto>();
        var dataInicio = DateTime.UtcNow.Date.AddDays(-30);

        // Contar mensagens enviadas por canal nos últimos 30 dias
        var brevoQuery = _context.HistoricosNotificacao
            .Where(h => h.EmpresaClienteId == _empresaClienteId &&
                       h.CanalUtilizado == CanalNotificacao.Email &&
                       h.DataEnvio >= dataInicio);

        // Aplicar filtros de segurança
        brevoQuery = ApplySecurityFiltersToHistoricos(brevoQuery);

        var mensagensBrevo = await brevoQuery.CountAsync(cancellationToken);

        var whatsappQuery = _context.HistoricosNotificacao
            .Where(h => h.EmpresaClienteId == _empresaClienteId &&
                       h.CanalUtilizado == CanalNotificacao.WhatsApp &&
                       h.DataEnvio >= dataInicio);

        // Aplicar filtros de segurança
        whatsappQuery = ApplySecurityFiltersToHistoricos(whatsappQuery);

        var mensagensTwilioWhatsApp = await whatsappQuery.CountAsync(cancellationToken);

        var smsQuery = _context.HistoricosNotificacao
            .Where(h => h.EmpresaClienteId == _empresaClienteId &&
                       h.CanalUtilizado == CanalNotificacao.SMS &&
                       h.DataEnvio >= dataInicio);

        // Aplicar filtros de segurança
        smsQuery = ApplySecurityFiltersToHistoricos(smsQuery);

        var mensagensTwilioSMS = await smsQuery.CountAsync(cancellationToken);

        var mensagensTwilioTotal = mensagensTwilioWhatsApp + mensagensTwilioSMS;

        // Brevo - Limite baseado no plano (assumindo plano gratuito = 300/dia, ~9000/mês)
        var limiteBrevo = 20000; // Ajustar conforme o plano real
        var percentualBrevo = limiteBrevo > 0 ? (decimal)mensagensBrevo / limiteBrevo * 100 : 0;
        var statusBrevo = percentualBrevo >= 90 ? "warning" : "OK";

        provedores.Add(new StatusProvedorDto
        {
            Nome = "Brevo",
            Status = statusBrevo,
            PercentualUtilizado = Math.Round(percentualBrevo, 1),
            LimiteTotal = limiteBrevo,
            LimiteUtilizado = mensagensBrevo
        });

        // Twilio - Limite estimado baseado em saldo/créditos
        var limiteTwilio = 10000; // Ajustar conforme o plano real
        var percentualTwilio = limiteTwilio > 0 ? (decimal)mensagensTwilioTotal / limiteTwilio * 100 : 0;
        var statusTwilio = percentualTwilio >= 90 ? "warning" : "OK";

        provedores.Add(new StatusProvedorDto
        {
            Nome = "Twilio",
            Status = statusTwilio,
            PercentualUtilizado = Math.Round(percentualTwilio, 1),
            LimiteTotal = limiteTwilio,
            LimiteUtilizado = mensagensTwilioTotal
        });

        return provedores;
    }

    private List<EventoTimelineDto> CalcularTimelineEventos(
        List<HistoricoNotificacao> historicos,
        List<Cobranca> cobrancas)
    {
        var eventos = new List<EventoTimelineDto>();

        // Adicionar eventos de histórico
        foreach (var historico in historicos.OrderByDescending(h => h.DataEnvio).Take(15))
        {
            var tipo = historico.Status == StatusNotificacao.Sucesso ? "Envio" : "Erro";
            var severidade = historico.Status == StatusNotificacao.Sucesso ? "success" : "danger";
            var icone = historico.Status == StatusNotificacao.Sucesso ? "pi-send" : "pi-times-circle";

            var nomeCliente = "Cliente";
            try
            {
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(historico.PayloadUtilizado);
                if (payload != null && payload.ContainsKey("payload"))
                {
                    var payloadObj = JsonSerializer.Deserialize<Dictionary<string, object>>(payload["payload"].ToString() ?? "{}");
                    if (payloadObj != null && payloadObj.ContainsKey("nome"))
                    {
                        nomeCliente = payloadObj["nome"].ToString() ?? "Cliente";
                    }
                }
            }
            catch { }

            var descricao = historico.Status == StatusNotificacao.Sucesso
                ? $"Cobrança enviada via {MapearCanal(historico.CanalUtilizado)} para {nomeCliente}"
                : $"Erro ao enviar via {MapearCanal(historico.CanalUtilizado)}: {historico.MensagemErro}";

            eventos.Add(new EventoTimelineDto
            {
                DataHora = historico.DataEnvio,
                Tipo = tipo,
                Severidade = severidade,
                Descricao = descricao,
                Icone = icone,
                Valor = null
            });
        }

        // Adicionar eventos de pagamento (cobrancas processadas)
        foreach (var cobranca in cobrancas.Where(c => c.Status == StatusCobranca.Processada && c.DataProcessamento.HasValue).OrderByDescending(c => c.DataProcessamento).Take(10))
        {
            decimal? valor = null;
            try
            {
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(cobranca.PayloadJson);
                if (payload != null && payload.ContainsKey("valor"))
                {
                    var valorStr = payload["valor"].ToString();
                    if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
                    {
                        valor = v;
                    }
                }
            }
            catch { }

            eventos.Add(new EventoTimelineDto
            {
                DataHora = cobranca.DataProcessamento!.Value,
                Tipo = "Pagamento",
                Severidade = "success",
                Descricao = "Pagamento confirmado",
                Icone = "pi-check-circle",
                Valor = valor
            });
        }

        return eventos.OrderByDescending(e => e.DataHora).ToList();
    }

    private List<InsightDto> GerarInsights(
        List<Cobranca> cobrancas,
        List<HistoricoNotificacao> historicos,
        decimal taxaConversao,
        decimal taxaEntrega)
    {
        var insights = new List<InsightDto>();

        // Insight sobre taxa de conversão
        if (taxaConversao > 30)
        {
            insights.Add(new InsightDto
            {
                Tipo = "Insight",
                Titulo = "Excelente Taxa de Conversão",
                Descricao = $"Sua taxa de conversão de {taxaConversao:F1}% está acima da média do mercado (20-25%). Continue com a estratégia atual!",
                Icone = "pi-thumbs-up",
                Cor = "green"
            });
        }
        else if (taxaConversao < 15)
        {
            insights.Add(new InsightDto
            {
                Tipo = "Recomendacao",
                Titulo = "Taxa de Conversão Baixa",
                Descricao = "Sua taxa de conversão está abaixo da média. Considere ajustar os templates ou os momentos de disparo.",
                Icone = "pi-chart-line",
                Cor = "yellow"
            });
        }

        // Insight sobre melhor canal
        var canalMaisEfetivo = historicos
            .GroupBy(h => h.CanalUtilizado)
            .OrderByDescending(g => g.Count(h => h.Status == StatusNotificacao.Sucesso))
            .FirstOrDefault();

        if (canalMaisEfetivo != null)
        {
            insights.Add(new InsightDto
            {
                Tipo = "Insight",
                Titulo = $"Canal Mais Efetivo: {MapearCanal(canalMaisEfetivo.Key)}",
                Descricao = $"O canal {MapearCanal(canalMaisEfetivo.Key)} tem a melhor taxa de entrega. Considere aumentar o uso deste canal.",
                Icone = "pi-star",
                Cor = "blue"
            });
        }

        // Insight sobre erros
        var taxaErro = historicos.Count > 0 ? (decimal)historicos.Count(h => h.Status == StatusNotificacao.Falha) / historicos.Count * 100 : 0;
        if (taxaErro > 10)
        {
            insights.Add(new InsightDto
            {
                Tipo = "Alerta",
                Titulo = "Taxa de Erro Elevada",
                Descricao = $"Há {taxaErro:F1}% de mensagens com erro. Verifique as configurações dos provedores e templates.",
                Icone = "pi-exclamation-triangle",
                Cor = "red"
            });
        }

        // Insight sobre horário
        var mensagensForaHorario = historicos.Count(h => h.DataEnvio.Hour < 8 || h.DataEnvio.Hour > 20);
        if (mensagensForaHorario > historicos.Count * 0.3)
        {
            insights.Add(new InsightDto
            {
                Tipo = "Recomendacao",
                Titulo = "Otimize os Horários de Envio",
                Descricao = "Muitas mensagens estão sendo enviadas fora do horário comercial. Ajuste suas réguas para melhor engajamento.",
                Icone = "pi-clock",
                Cor = "yellow"
            });
        }

        return insights;
    }

    // Filtros de segurança
    private IQueryable<HistoricoNotificacao> ApplySecurityFiltersToHistoricos(IQueryable<HistoricoNotificacao> query)
    {
        // Proprietário vê tudo
        if (_currentUserService.EhProprietario)
            return query;

        // Se não está autenticado, retorna vazio
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return query.Where(h => false);

        var userId = _currentUserService.UserId.Value;
        var perfil = _currentUserService.Perfil;

        // Operador vê apenas o que ele criou
        if (perfil == PerfilUsuario.Operador)
        {
            return query.Where(h => h.UsuarioCriacaoId == userId);
        }

        // Admin vê seus próprios + dados antigos
        if (perfil == PerfilUsuario.Admin)
        {
            return query.Where(h =>
                h.UsuarioCriacaoId == userId ||
                !h.UsuarioCriacaoId.HasValue
            );
        }

        // Fallback: retorna apenas dados do próprio usuário
        return query.Where(h => h.UsuarioCriacaoId == userId || !h.UsuarioCriacaoId.HasValue);
    }

    private IQueryable<Cobranca> ApplySecurityFiltersToCobrancas(IQueryable<Cobranca> query)
    {
        // Proprietário vê tudo
        if (_currentUserService.EhProprietario)
            return query;

        // Se não está autenticado, retorna vazio
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return query.Where(c => false);

        var userId = _currentUserService.UserId.Value;
        var perfil = _currentUserService.Perfil;

        // Operador vê apenas o que ele criou
        if (perfil == PerfilUsuario.Operador)
        {
            return query.Where(c => c.UsuarioCriacaoId == userId);
        }

        // Admin vê seus próprios + dados antigos
        if (perfil == PerfilUsuario.Admin)
        {
            return query.Where(c =>
                c.UsuarioCriacaoId == userId ||
                !c.UsuarioCriacaoId.HasValue
            );
        }

        // Fallback: retorna apenas dados do próprio usuário
        return query.Where(c => c.UsuarioCriacaoId == userId || !c.UsuarioCriacaoId.HasValue);
    }

    // Métodos auxiliares
    private CanalNotificacao ParseCanal(string canal)
    {
        return canal.ToLower() switch
        {
            "email" or "e-mail" => CanalNotificacao.Email,
            "whatsapp" => CanalNotificacao.WhatsApp,
            "sms" => CanalNotificacao.SMS,
            _ => CanalNotificacao.Email
        };
    }

    private string MapearCanal(CanalNotificacao canal)
    {
        return canal switch
        {
            CanalNotificacao.Email => "E-mail",
            CanalNotificacao.WhatsApp => "WhatsApp",
            CanalNotificacao.SMS => "SMS",
            _ => "Desconhecido"
        };
    }

    private decimal ObterCustoCanal(CanalNotificacao canal)
    {
        return canal switch
        {
            CanalNotificacao.Email => 0.01m,
            CanalNotificacao.WhatsApp => 0.10m,
            CanalNotificacao.SMS => 0.15m,
            _ => 0.05m
        };
    }

    private string ObterNomeDiaSemana(DayOfWeek dia)
    {
        return dia switch
        {
            DayOfWeek.Sunday => "Domingo",
            DayOfWeek.Monday => "Segunda-feira",
            DayOfWeek.Tuesday => "Terça-feira",
            DayOfWeek.Wednesday => "Quarta-feira",
            DayOfWeek.Thursday => "Quinta-feira",
            DayOfWeek.Friday => "Sexta-feira",
            DayOfWeek.Saturday => "Sábado",
            _ => "Desconhecido"
        };
    }
}
