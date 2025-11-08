using Cobrio.Application.DTOs.Relatorios;
using Cobrio.Domain.Enums;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Cobrio.API.Services;

public class RelatoriosAvancadosService
{
    private readonly CobrioDbContext _context;
    private readonly ILogger<RelatoriosAvancadosService> _logger;

    public RelatoriosAvancadosService(
        CobrioDbContext context,
        ILogger<RelatoriosAvancadosService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ========================================================================
    // RELATÓRIOS OPERACIONAIS
    // ========================================================================

    public async Task<DashboardOperacionalResponse> GetDashboardOperacionalAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        Guid? regraCobrancaId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Período anterior para comparação
            var diasPeriodo = (dataFim - dataInicio).Days;
            var dataInicioPeriodoAnterior = dataInicio.AddDays(-diasPeriodo);
            var dataFimPeriodoAnterior = dataInicio.AddSeconds(-1);

            // Query base
            var queryAtual = _context.Cobrancas
                .Where(c => c.EmpresaClienteId == empresaClienteId &&
                           c.DataProcessamento >= dataInicio &&
                           c.DataProcessamento <= dataFim);

            if (regraCobrancaId.HasValue)
                queryAtual = queryAtual.Where(c => c.RegraCobrancaId == regraCobrancaId.Value);

            var cobrancasAtual = await queryAtual.ToListAsync(cancellationToken);

            var totalCobrancas = cobrancasAtual.Count;
            var cobrancasProcessadas = cobrancasAtual.Count(c => c.Status == StatusCobranca.Processada);
            var cobrancasFalhas = cobrancasAtual.Count(c => c.Status == StatusCobranca.Falha);
            var cobrancasPendentes = cobrancasAtual.Count(c => c.Status == StatusCobranca.Pendente);

            var valorTotal = cobrancasAtual.Sum(c => ExtractValorFromPayload(c.PayloadJson));
            var valorProcessado = cobrancasAtual
                .Where(c => c.Status == StatusCobranca.Processada)
                .Sum(c => ExtractValorFromPayload(c.PayloadJson));

            var mediaTentativas = totalCobrancas > 0
                ? cobrancasAtual.Average(c => (decimal)c.TentativasEnvio)
                : 0;

            var cobrancasRetentadas = cobrancasAtual.Count(c => c.TentativasEnvio > 1);

            // Período anterior
            var queryAnterior = _context.Cobrancas
                .Where(c => c.EmpresaClienteId == empresaClienteId &&
                           c.DataProcessamento >= dataInicioPeriodoAnterior &&
                           c.DataProcessamento <= dataFimPeriodoAnterior &&
                           c.Status == StatusCobranca.Processada);

            if (regraCobrancaId.HasValue)
                queryAnterior = queryAnterior.Where(c => c.RegraCobrancaId == regraCobrancaId.Value);

            var cobrancasProcessadasAnterior = await queryAnterior.CountAsync(cancellationToken);

            var variacaoProcessadas = CalcularVariacaoPercentual(cobrancasProcessadas, cobrancasProcessadasAnterior);
            var taxaSucesso = totalCobrancas > 0
                ? Math.Round((decimal)cobrancasProcessadas / totalCobrancas * 100, 2)
                : 0;

            return new DashboardOperacionalResponse
            {
                TotalCobrancas = totalCobrancas,
                CobrancasProcessadas = cobrancasProcessadas,
                CobrancasFalhas = cobrancasFalhas,
                CobrancasPendentes = cobrancasPendentes,
                ValorTotal = valorTotal,
                ValorProcessado = valorProcessado,
                MediaTentativas = mediaTentativas,
                CobrancasRetentadas = cobrancasRetentadas,
                VariacaoProcessadas = variacaoProcessadas,
                TaxaSucesso = taxaSucesso
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dashboard operacional");
            throw;
        }
    }

    public async Task<List<ExecucaoReguaResponse>> GetExecucaoReguasAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CanalNotificacao? canal = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = from hn in _context.HistoricosNotificacao
                        join c in _context.Cobrancas on hn.CobrancaId equals c.Id
                        join rc in _context.RegrasCobranca on hn.RegraCobrancaId equals rc.Id
                        where hn.EmpresaClienteId == empresaClienteId &&
                              hn.DataEnvio >= dataInicio &&
                              hn.DataEnvio <= dataFim
                        select new
                        {
                            Data = hn.DataEnvio.Date,
                            hn.CanalUtilizado,
                            rc.Nome,
                            rc.Id,
                            HnStatus = hn.Status,
                            CobrancaStatus = c.Status
                        };

            if (canal.HasValue)
                query = query.Where(x => x.CanalUtilizado == canal.Value);

            var dados = await query.ToListAsync(cancellationToken);

            var resultado = dados
                .GroupBy(x => new { x.Data, x.CanalUtilizado, x.Nome, x.Id })
                .Select(g => new ExecucaoReguaResponse
                {
                    Data = g.Key.Data,
                    Canal = g.Key.CanalUtilizado,
                    NomeRegra = g.Key.Nome,
                    RegraId = g.Key.Id,
                    TotalEnvios = g.Count(),
                    Sucessos = g.Count(x => x.HnStatus == StatusNotificacao.Sucesso),
                    Falhas = g.Count(x => x.HnStatus == StatusNotificacao.Falha),
                    TaxaSucesso = g.Count() > 0
                        ? Math.Round((decimal)g.Count(x => x.HnStatus == StatusNotificacao.Sucesso) / g.Count() * 100, 2)
                        : 0,
                    CobrancasProcessadas = g.Count(x => x.CobrancaStatus == StatusCobranca.Processada),
                    CobrancasFalhas = g.Count(x => x.CobrancaStatus == StatusCobranca.Falha)
                })
                .OrderByDescending(x => x.Data)
                .ThenByDescending(x => x.TotalEnvios)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar execução de réguas");
            throw;
        }
    }

    public async Task<EntregasFalhasResponse> GetEntregasFalhasAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Falhas por tipo
            var cobrancasFalhas = await _context.Cobrancas
                .Where(c => c.EmpresaClienteId == empresaClienteId &&
                           c.DataProcessamento >= dataInicio &&
                           c.DataProcessamento <= dataFim &&
                           c.Status == StatusCobranca.Falha)
                .Select(c => new
                {
                    Data = c.DataProcessamento!.Value.Date,
                    c.MensagemErro,
                    c.RegraCobrancaId
                })
                .ToListAsync(cancellationToken);

            var falhasPorTipo = cobrancasFalhas
                .GroupBy(c => new
                {
                    c.Data,
                    TipoErro = ClassificarTipoErro(c.MensagemErro)
                })
                .Select(g => new FalhasPorTipoResponse
                {
                    Data = g.Key.Data,
                    TipoErro = g.Key.TipoErro,
                    Quantidade = g.Count(),
                    RegrasAfetadas = g.Select(x => x.RegraCobrancaId).Distinct().ToList()
                })
                .OrderByDescending(x => x.Data)
                .ThenByDescending(x => x.Quantidade)
                .ToList();

            // Pendências por tempo
            var cobrancasPendentes = await _context.Cobrancas
                .Where(c => c.EmpresaClienteId == empresaClienteId &&
                           c.Status == StatusCobranca.Pendente)
                .Select(c => new
                {
                    c.DataDisparo,
                    c.PayloadJson
                })
                .ToListAsync(cancellationToken);

            var agora = DateTime.UtcNow;
            var pendenciasPorTempo = cobrancasPendentes
                .Select(c => new
                {
                    Horas = (agora - c.DataDisparo).TotalHours,
                    Valor = ExtractValorFromPayload(c.PayloadJson)
                })
                .GroupBy(c => c.Horas switch
                {
                    < 24 => "0-24h",
                    < 72 => "24-72h",
                    < 168 => "3-7 dias",
                    _ => "Mais de 7 dias"
                })
                .Select(g => new PendenciasPorTempoResponse
                {
                    FaixaTempo = g.Key,
                    Quantidade = g.Count(),
                    ValorTotal = g.Sum(x => x.Valor)
                })
                .ToList();

            return new EntregasFalhasResponse
            {
                FalhasPorTipo = falhasPorTipo,
                PendenciasPorTempo = pendenciasPorTempo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar entregas e falhas");
            throw;
        }
    }

    public async Task<List<CobrancasRecebimentosResponse>> GetCobrancasRecebimentosAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dados = await (from c in _context.Cobrancas
                              where c.EmpresaClienteId == empresaClienteId &&
                                    c.DataProcessamento >= dataInicio &&
                                    c.DataProcessamento <= dataFim &&
                                    c.Status == StatusCobranca.Processada
                              select new
                              {
                                  Data = c.DataProcessamento!.Value.Date,
                                  c.Id,
                                  c.PayloadJson,
                                  c.DataProcessamento
                              }).ToListAsync(cancellationToken);

            var faturaIds = dados
                .Select(c => ExtractFaturaIdFromPayload(c.PayloadJson))
                .Where(id => id != Guid.Empty)
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id))
                .Select(f => new
                {
                    f.Id,
                    f.Status,
                    ValorLiquido = f.ValorLiquido.Valor,
                    f.DataPagamento,
                    f.DataVencimento
                })
                .ToListAsync(cancellationToken);

            var resultado = dados
                .GroupBy(c => c.Data)
                .Select(g =>
                {
                    var faturaIdsGrupo = g
                        .Select(x => ExtractFaturaIdFromPayload(x.PayloadJson))
                        .Where(id => id != Guid.Empty)
                        .ToList();

                    var faturasGrupo = faturas.Where(f => faturaIdsGrupo.Contains(f.Id)).ToList();
                    var faturasPagas = faturasGrupo.Where(f => f.Status == StatusFatura.Pago).ToList();
                    var faturasVencidas = faturasGrupo.Count(f =>
                        f.DataVencimento < DateTime.UtcNow &&
                        (f.Status == StatusFatura.Pendente || f.Status == StatusFatura.AguardandoPagamento));

                    var valorRecebido = faturasPagas.Sum(f => f.ValorLiquido);
                    var valorCobrado = g.Sum(x => ExtractValorFromPayload(x.PayloadJson));

                    var mediaHoras = faturasPagas.Any()
                        ? faturasPagas.Average(f =>
                        {
                            var cobranca = g.FirstOrDefault(c => ExtractFaturaIdFromPayload(c.PayloadJson) == f.Id);
                            if (cobranca != null && f.DataPagamento.HasValue)
                                return (f.DataPagamento.Value - cobranca.DataProcessamento!.Value).TotalHours;
                            return 0.0;
                        })
                        : 0;

                    return new CobrancasRecebimentosResponse
                    {
                        Data = g.Key,
                        TotalCobrancasEnviadas = g.Count(),
                        ValorCobrado = valorCobrado,
                        TotalFaturasGeradas = faturasGrupo.Count,
                        FaturasPagas = faturasPagas.Count,
                        ValorRecebido = valorRecebido,
                        TaxaRecuperacao = valorCobrado > 0
                            ? Math.Round(valorRecebido / valorCobrado * 100, 2)
                            : 0,
                        MediaHorasAtePagamento = Math.Round((decimal)mediaHoras, 2),
                        FaturasVencidas = faturasVencidas
                    };
                })
                .OrderByDescending(x => x.Data)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cobranças x recebimentos");
            throw;
        }
    }

    public async Task<List<ValoresPorReguaResponse>> GetValoresPorReguaAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dados = await (from rc in _context.RegrasCobranca
                              join c in _context.Cobrancas on rc.Id equals c.RegraCobrancaId
                              where rc.EmpresaClienteId == empresaClienteId &&
                                    c.DataProcessamento >= dataInicio &&
                                    c.DataProcessamento <= dataFim &&
                                    c.Status == StatusCobranca.Processada
                              select new
                              {
                                  rc.Id,
                                  rc.Nome,
                                  rc.CanalNotificacao,
                                  rc.TipoMomento,
                                  rc.ValorTempo,
                                  rc.UnidadeTempo,
                                  CobrancaId = c.Id,
                                  c.PayloadJson
                              }).ToListAsync(cancellationToken);

            var faturaIds = dados
                .Select(d => ExtractFaturaIdFromPayload(d.PayloadJson))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id) && f.Status == StatusFatura.Pago)
                .Select(f => new { f.Id, ValorLiquido = f.ValorLiquido.Valor })
                .ToListAsync(cancellationToken);

            var resultado = dados
                .GroupBy(d => new
                {
                    d.Id,
                    d.Nome,
                    d.CanalNotificacao,
                    d.TipoMomento,
                    d.ValorTempo,
                    d.UnidadeTempo
                })
                .Select(g =>
                {
                    var totalCobrancas = g.Count();
                    var valorCobrado = g.Sum(x => ExtractValorFromPayload(x.PayloadJson));

                    var faturaIdsGrupo = g
                        .Select(x => ExtractFaturaIdFromPayload(x.PayloadJson))
                        .Where(id => id != Guid.Empty)
                        .ToList();

                    var faturasGrupo = faturas.Where(f => faturaIdsGrupo.Contains(f.Id)).ToList();
                    var valorRecuperado = faturasGrupo.Sum(f => f.ValorLiquido);

                    var custoEstimado = totalCobrancas * ObterCustoPorCanal(g.Key.CanalNotificacao);
                    var roi = custoEstimado > 0
                        ? Math.Round((valorRecuperado - custoEstimado) / custoEstimado * 100, 2)
                        : 0;

                    return new ValoresPorReguaResponse
                    {
                        RegraId = g.Key.Id,
                        NomeRegra = g.Key.Nome,
                        Canal = g.Key.CanalNotificacao,
                        DescricaoTiming = FormatarTiming(g.Key.TipoMomento, g.Key.ValorTempo, g.Key.UnidadeTempo),
                        TotalCobrancas = totalCobrancas,
                        ValorCobrado = valorCobrado,
                        FaturasPagas = faturasGrupo.Count,
                        ValorRecuperado = valorRecuperado,
                        TaxaRecuperacao = valorCobrado > 0
                            ? Math.Round(valorRecuperado / valorCobrado * 100, 2)
                            : 0,
                        CustoEstimado = custoEstimado,
                        ROI = roi
                    };
                })
                .OrderByDescending(x => x.ValorRecuperado)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar valores por régua");
            throw;
        }
    }

    public async Task<List<PagamentosPorAtrasoResponse>> GetPagamentosPorAtrasoAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var faturas = await _context.Faturas
                .Where(f => f.EmpresaClienteId == empresaClienteId &&
                           f.Status == StatusFatura.Pago &&
                           f.DataPagamento >= dataInicio &&
                           f.DataPagamento <= dataFim)
                .Select(f => new
                {
                    f.Id,
                    f.DataVencimento,
                    f.DataPagamento,
                    ValorLiquido = f.ValorLiquido.Valor
                })
                .ToListAsync(cancellationToken);

            var faturaIds = faturas.Select(f => f.Id).ToList();

            var cobrancasPorFatura = await _context.Cobrancas
                .Where(c => c.Status == StatusCobranca.Processada)
                .Select(c => new
                {
                    FaturaId = ExtractFaturaIdFromPayload(c.PayloadJson),
                    c.DataProcessamento
                })
                .Where(c => faturaIds.Contains(c.FaturaId))
                .ToListAsync(cancellationToken);

            var resultado = faturas
                .Select(f =>
                {
                    var diasAtraso = (f.DataPagamento!.Value - f.DataVencimento).Days;
                    var faixaAtraso = diasAtraso switch
                    {
                        <= 0 => "Em dia",
                        <= 7 => "1-7 dias",
                        <= 15 => "8-15 dias",
                        <= 30 => "16-30 dias",
                        <= 60 => "31-60 dias",
                        _ => "Mais de 60 dias"
                    };

                    var cobrancas = cobrancasPorFatura
                        .Where(c => c.FaturaId == f.Id && c.DataProcessamento <= f.DataPagamento)
                        .OrderBy(c => c.DataProcessamento)
                        .ToList();

                    var diasAteRecuperacao = cobrancas.Any() && f.DataPagamento.HasValue
                        ? (f.DataPagamento.Value - cobrancas.First().DataProcessamento!.Value).Days
                        : 0;

                    return new
                    {
                        FaixaAtraso = faixaAtraso,
                        ValorLiquido = f.ValorLiquido,
                        MediaCobrancas = (decimal)cobrancas.Count,
                        DiasAteRecuperacao = (decimal)diasAteRecuperacao
                    };
                })
                .GroupBy(x => x.FaixaAtraso)
                .Select(g => new PagamentosPorAtrasoResponse
                {
                    FaixaAtraso = g.Key,
                    QuantidadeFaturas = g.Count(),
                    ValorTotal = g.Sum(x => x.ValorLiquido),
                    TicketMedio = g.Any() ? Math.Round(g.Average(x => x.ValorLiquido), 2) : 0,
                    MediaCobrancasEnviadas = g.Any() ? Math.Round(g.Average(x => x.MediaCobrancas), 2) : 0,
                    MediaDiasAteRecuperacao = g.Any() ? Math.Round(g.Average(x => x.DiasAteRecuperacao), 2) : 0
                })
                .ToList();

            // Ordenar na ordem correta
            var ordem = new[] { "Em dia", "1-7 dias", "8-15 dias", "16-30 dias", "31-60 dias", "Mais de 60 dias" };
            resultado = resultado.OrderBy(r => Array.IndexOf(ordem, r.FaixaAtraso)).ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pagamentos por atraso");
            throw;
        }
    }

    // ========================================================================
    // RELATÓRIOS GERENCIAIS
    // ========================================================================

    public async Task<List<ConversaoPorCanalResponse>> GetConversaoPorCanalAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dados = await (from hn in _context.HistoricosNotificacao
                              join c in _context.Cobrancas on hn.CobrancaId equals c.Id
                              join rc in _context.RegrasCobranca on hn.RegraCobrancaId equals rc.Id
                              where hn.EmpresaClienteId == empresaClienteId &&
                                    hn.DataEnvio >= dataInicio &&
                                    hn.DataEnvio <= dataFim
                              select new
                              {
                                  rc.Nome,
                                  hn.CanalUtilizado,
                                  hn.Id,
                                  hn.CobrancaId,
                                  CobrancaStatus = c.Status,
                                  c.PayloadJson,
                                  hn.DataEnvio
                              }).ToListAsync(cancellationToken);

            var faturaIds = dados
                .Select(d => ExtractFaturaIdFromPayload(d.PayloadJson))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id) && f.Status == StatusFatura.Pago)
                .Select(f => new { f.Id, ValorLiquido = f.ValorLiquido.Valor, f.DataPagamento })
                .ToListAsync(cancellationToken);

            var resultado = dados
                .GroupBy(d => new { d.Nome, d.CanalUtilizado })
                .Select(g =>
                {
                    var totalEnvios = g.Count();
                    var totalCobrancas = g.Select(x => x.CobrancaId).Distinct().Count();
                    var cobrancasProcessadas = g.Where(x => x.CobrancaStatus == StatusCobranca.Processada)
                        .Select(x => x.CobrancaId).Distinct().Count();

                    var faturaIdsGrupo = g
                        .Select(x => ExtractFaturaIdFromPayload(x.PayloadJson))
                        .Where(id => id != Guid.Empty)
                        .Distinct()
                        .ToList();

                    var faturasGrupo = faturas.Where(f => faturaIdsGrupo.Contains(f.Id)).ToList();
                    var faturasPagas = faturasGrupo.Count;
                    var valorRecuperado = faturasGrupo.Sum(f => f.ValorLiquido);
                    var ticketMedio = faturasPagas > 0
                        ? Math.Round(valorRecuperado / faturasPagas, 2)
                        : 0;

                    var mediaHoras = faturasGrupo.Any()
                        ? faturasGrupo.Average(f =>
                        {
                            var envio = g.FirstOrDefault(x => ExtractFaturaIdFromPayload(x.PayloadJson) == f.Id);
                            if (envio != null && f.DataPagamento.HasValue)
                                return (f.DataPagamento.Value - envio.DataEnvio).TotalHours;
                            return 0.0;
                        })
                        : 0;

                    return new ConversaoPorCanalResponse
                    {
                        NomeRegra = g.Key.Nome,
                        Canal = g.Key.CanalUtilizado,
                        Funil = new FunilConversaoDto
                        {
                            TotalEnvios = totalEnvios,
                            TotalCobrancas = totalCobrancas,
                            CobrancasProcessadas = cobrancasProcessadas,
                            FaturasPagas = faturasPagas
                        },
                        TaxaProcessamento = totalCobrancas > 0
                            ? Math.Round((decimal)cobrancasProcessadas / totalCobrancas * 100, 2)
                            : 0,
                        TaxaConversaoFinal = totalCobrancas > 0
                            ? Math.Round((decimal)faturasPagas / totalCobrancas * 100, 2)
                            : 0,
                        ValorRecuperado = valorRecuperado,
                        TicketMedio = ticketMedio,
                        MediaHorasConversao = Math.Round((decimal)mediaHoras, 2)
                    };
                })
                .OrderByDescending(x => x.TaxaConversaoFinal)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conversão por canal");
            throw;
        }
    }

    public async Task<List<ROIReguasResponse>> GetROIReguasAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dados = await (from rc in _context.RegrasCobranca
                              join c in _context.Cobrancas on rc.Id equals c.RegraCobrancaId
                              where rc.EmpresaClienteId == empresaClienteId &&
                                    c.DataProcessamento >= dataInicio &&
                                    c.DataProcessamento <= dataFim &&
                                    c.Status == StatusCobranca.Processada
                              select new
                              {
                                  Periodo = c.DataProcessamento!.Value.ToString("yyyy-MM"),
                                  rc.Id,
                                  rc.Nome,
                                  rc.CanalNotificacao,
                                  c.PayloadJson
                              }).ToListAsync(cancellationToken);

            var faturaIds = dados
                .Select(d => ExtractFaturaIdFromPayload(d.PayloadJson))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id) && f.Status == StatusFatura.Pago)
                .Select(f => new { f.Id, ValorLiquido = f.ValorLiquido.Valor })
                .ToListAsync(cancellationToken);

            var resultado = dados
                .GroupBy(d => new
                {
                    d.Periodo,
                    d.Id,
                    d.Nome,
                    d.CanalNotificacao
                })
                .Select(g =>
                {
                    var totalEnvios = g.Count();
                    var custoTotal = totalEnvios * ObterCustoPorCanal(g.Key.CanalNotificacao);

                    var faturaIdsGrupo = g
                        .Select(x => ExtractFaturaIdFromPayload(x.PayloadJson))
                        .Where(id => id != Guid.Empty)
                        .ToList();

                    var faturasGrupo = faturas.Where(f => faturaIdsGrupo.Contains(f.Id)).ToList();
                    var receitaRecuperada = faturasGrupo.Sum(f => f.ValorLiquido);
                    var lucroLiquido = receitaRecuperada - custoTotal;
                    var roi = custoTotal > 0
                        ? Math.Round((receitaRecuperada - custoTotal) / custoTotal * 100, 2)
                        : 0;

                    return new ROIReguasResponse
                    {
                        Periodo = g.Key.Periodo,
                        RegraId = g.Key.Id,
                        NomeRegra = g.Key.Nome,
                        Canal = g.Key.CanalNotificacao,
                        TotalEnvios = totalEnvios,
                        CustoTotal = custoTotal,
                        ReceitaRecuperada = receitaRecuperada,
                        ROI = roi,
                        LucroLiquido = lucroLiquido
                    };
                })
                .OrderByDescending(x => x.Periodo)
                .ThenByDescending(x => x.ROI)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ROI de réguas");
            throw;
        }
    }

    public async Task<List<EvolucaoMensalResponse>> GetEvolucaoMensalAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cobrancas = await _context.Cobrancas
                .Where(c => c.EmpresaClienteId == empresaClienteId &&
                           c.DataProcessamento >= dataInicio &&
                           c.DataProcessamento <= dataFim)
                .Select(c => new
                {
                    Periodo = c.DataProcessamento!.Value.ToString("yyyy-MM"),
                    c.Id,
                    c.Status,
                    c.PayloadJson
                })
                .ToListAsync(cancellationToken);

            var cobrancasIds = cobrancas.Select(c => c.Id).ToList();

            var notificacoes = await _context.HistoricosNotificacao
                .Where(h => cobrancasIds.Contains(h.CobrancaId))
                .Select(h => new
                {
                    h.CobrancaId,
                    h.CanalUtilizado
                })
                .ToListAsync(cancellationToken);

            var faturaIds = cobrancas
                .Select(c => ExtractFaturaIdFromPayload(c.PayloadJson))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id))
                .Select(f => new
                {
                    f.Id,
                    f.Status,
                    ValorLiquido = f.ValorLiquido.Valor,
                    f.DataVencimento
                })
                .ToListAsync(cancellationToken);

            var agora = DateTime.UtcNow;

            var resultado = cobrancas
                .GroupBy(c => c.Periodo)
                .Select(g =>
                {
                    var totalCobrancas = g.Count();
                    var cobrancasProcessadas = g.Count(c => c.Status == StatusCobranca.Processada);
                    var valorCobrado = g.Sum(c => ExtractValorFromPayload(c.PayloadJson));

                    var faturaIdsGrupo = g
                        .Select(c => ExtractFaturaIdFromPayload(c.PayloadJson))
                        .Where(id => id != Guid.Empty)
                        .ToList();

                    var faturasGrupo = faturas.Where(f => faturaIdsGrupo.Contains(f.Id)).ToList();
                    var faturasPagas = faturasGrupo.Count(f => f.Status == StatusFatura.Pago);
                    var valorRecebido = faturasGrupo.Where(f => f.Status == StatusFatura.Pago).Sum(f => f.ValorLiquido);
                    var faturasInadimplentes = faturasGrupo.Count(f =>
                        f.DataVencimento < agora &&
                        (f.Status == StatusFatura.Pendente || f.Status == StatusFatura.AguardandoPagamento));

                    var cobrancasIdsGrupo = g.Select(c => c.Id).ToList();
                    var notificacoesGrupo = notificacoes.Where(n => cobrancasIdsGrupo.Contains(n.CobrancaId)).ToList();

                    return new EvolucaoMensalResponse
                    {
                        Periodo = g.Key,
                        TotalCobrancas = totalCobrancas,
                        CobrancasProcessadas = cobrancasProcessadas,
                        ValorCobrado = valorCobrado,
                        FaturasPagas = faturasPagas,
                        ValorRecebido = valorRecebido,
                        TaxaSucesso = totalCobrancas > 0
                            ? Math.Round((decimal)cobrancasProcessadas / totalCobrancas * 100, 2)
                            : 0,
                        TaxaConversao = totalCobrancas > 0
                            ? Math.Round((decimal)faturasPagas / totalCobrancas * 100, 2)
                            : 0,
                        FaturasInadimplentes = faturasInadimplentes,
                        BreakdownCanal = new BreakdownCanalDto
                        {
                            EnviosEmail = notificacoesGrupo.Count(n => n.CanalUtilizado == CanalNotificacao.Email),
                            EnviosSMS = notificacoesGrupo.Count(n => n.CanalUtilizado == CanalNotificacao.SMS),
                            EnviosWhatsApp = notificacoesGrupo.Count(n => n.CanalUtilizado == CanalNotificacao.WhatsApp)
                        }
                    };
                })
                .OrderByDescending(x => x.Periodo)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evolução mensal");
            throw;
        }
    }

    public async Task<MelhorHorarioEnvioResponse> GetMelhorHorarioEnvioAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dados = await (from hn in _context.HistoricosNotificacao
                              join c in _context.Cobrancas on hn.CobrancaId equals c.Id
                              where hn.EmpresaClienteId == empresaClienteId &&
                                    hn.DataEnvio >= dataInicio &&
                                    hn.DataEnvio <= dataFim &&
                                    hn.Status == StatusNotificacao.Sucesso
                              select new
                              {
                                  hn.DataEnvio,
                                  hn.CobrancaId,
                                  c.PayloadJson
                              }).ToListAsync(cancellationToken);

            var faturaIds = dados
                .Select(d => ExtractFaturaIdFromPayload(d.PayloadJson))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id) && f.Status == StatusFatura.Pago)
                .Select(f => new { f.Id, f.DataPagamento })
                .ToListAsync(cancellationToken);

            // Por dia da semana
            var porDiaSemana = dados
                .Select(d => new
                {
                    DiaSemana = (int)d.DataEnvio.DayOfWeek + 1,
                    NomeDia = ObterNomeDiaSemana((int)d.DataEnvio.DayOfWeek + 1),
                    d.CobrancaId,
                    FaturaId = ExtractFaturaIdFromPayload(d.PayloadJson),
                    d.DataEnvio
                })
                .GroupBy(d => new { d.DiaSemana, d.NomeDia })
                .Select(g =>
                {
                    var totalCobrancas = g.Select(x => x.CobrancaId).Distinct().Count();
                    var faturasGrupo = faturas.Where(f => g.Select(x => x.FaturaId).Contains(f.Id)).ToList();
                    var faturasPagas = faturasGrupo.Count;

                    var mediaHoras = faturasGrupo.Any()
                        ? faturasGrupo.Average(f =>
                        {
                            var envio = g.FirstOrDefault(x => x.FaturaId == f.Id);
                            if (envio != null && f.DataPagamento.HasValue)
                                return (f.DataPagamento.Value - envio.DataEnvio).TotalHours;
                            return 0.0;
                        })
                        : 0;

                    return new PerformancePorDiaResponse
                    {
                        DiaSemana = g.Key.DiaSemana,
                        NomeDia = g.Key.NomeDia,
                        TotalCobrancas = totalCobrancas,
                        FaturasPagas = faturasPagas,
                        TaxaConversao = totalCobrancas > 0
                            ? Math.Round((decimal)faturasPagas / totalCobrancas * 100, 2)
                            : 0,
                        MediaHorasConversao = Math.Round((decimal)mediaHoras, 2)
                    };
                })
                .OrderByDescending(x => x.TaxaConversao)
                .ToList();

            // Por horário
            var porHoraDia = dados
                .Select(d => new
                {
                    Hora = d.DataEnvio.Hour,
                    Periodo = ClassificarPeriodo(d.DataEnvio.Hour),
                    d.CobrancaId,
                    FaturaId = ExtractFaturaIdFromPayload(d.PayloadJson),
                    d.DataEnvio
                })
                .GroupBy(d => new { d.Hora, d.Periodo })
                .Select(g =>
                {
                    var totalCobrancas = g.Select(x => x.CobrancaId).Distinct().Count();
                    var faturasGrupo = faturas.Where(f => g.Select(x => x.FaturaId).Contains(f.Id)).ToList();
                    var faturasPagas = faturasGrupo.Count;

                    var mediaHoras = faturasGrupo.Any()
                        ? faturasGrupo.Average(f =>
                        {
                            var envio = g.FirstOrDefault(x => x.FaturaId == f.Id);
                            if (envio != null && f.DataPagamento.HasValue)
                                return (f.DataPagamento.Value - envio.DataEnvio).TotalHours;
                            return 0.0;
                        })
                        : 0;

                    return new PerformancePorHoraResponse
                    {
                        Hora = g.Key.Hora,
                        Periodo = g.Key.Periodo,
                        TotalCobrancas = totalCobrancas,
                        FaturasPagas = faturasPagas,
                        TaxaConversao = totalCobrancas > 0
                            ? Math.Round((decimal)faturasPagas / totalCobrancas * 100, 2)
                            : 0,
                        MediaHorasConversao = Math.Round((decimal)mediaHoras, 2)
                    };
                })
                .OrderByDescending(x => x.TaxaConversao)
                .ToList();

            var recomendacao = new RecomendacaoEnvioDto();
            if (porDiaSemana.Any() && porHoraDia.Any())
            {
                var melhorDia = porDiaSemana.First();
                var melhorHora = porHoraDia.First();

                recomendacao.MelhorDia = melhorDia.NomeDia;
                recomendacao.MelhorPeriodo = $"Entre {melhorHora.Hora}h e {melhorHora.Hora + 1}h";
                recomendacao.TaxaConversaoEsperada = Math.Min(melhorDia.TaxaConversao, melhorHora.TaxaConversao);
            }

            return new MelhorHorarioEnvioResponse
            {
                PorDiaSemana = porDiaSemana,
                PorHoraDia = porHoraDia,
                Recomendacao = recomendacao
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar melhor horário de envio");
            throw;
        }
    }

    public async Task<ReducaoInadimplenciaResponse> GetReducaoInadimplenciaAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var faturas = await _context.Faturas
                .Where(f => f.EmpresaClienteId == empresaClienteId &&
                           f.DataVencimento >= dataInicio &&
                           f.DataVencimento <= dataFim)
                .Select(f => new
                {
                    f.Id,
                    f.Status,
                    f.DataVencimento,
                    f.DataPagamento,
                    ValorLiquido = f.ValorLiquido.Valor
                })
                .ToListAsync(cancellationToken);

            var faturaIds = faturas.Select(f => f.Id).ToList();

            var cobrancasPorFatura = await _context.Cobrancas
                .Where(c => c.Status == StatusCobranca.Processada)
                .Select(c => new
                {
                    FaturaId = ExtractFaturaIdFromPayload(c.PayloadJson),
                    c.Id
                })
                .Where(c => faturaIds.Contains(c.FaturaId))
                .ToListAsync(cancellationToken);

            var faturasComRegua = faturas
                .Where(f => cobrancasPorFatura.Any(c => c.FaturaId == f.Id))
                .ToList();

            var faturasSemRegua = faturas
                .Where(f => !cobrancasPorFatura.Any(c => c.FaturaId == f.Id))
                .ToList();

            var comRegua = CalcularMetricasComRegua(faturasComRegua);
            var semRegua = CalcularMetricasSemRegua(faturasSemRegua);

            var melhoriaPercentual = comRegua.TaxaPagamento - semRegua.TaxaPagamento;
            var reducaoDias = semRegua.MediaDiasAtraso - comRegua.MediaDiasAtraso;

            var interpretacao = melhoriaPercentual switch
            {
                >= 20 => "Alto",
                >= 10 => "Médio",
                _ => "Baixo"
            };

            return new ReducaoInadimplenciaResponse
            {
                ComRegua = comRegua,
                SemRegua = semRegua,
                Impacto = new ImpactoReguaDto
                {
                    PontoPercentualMelhoria = Math.Round(melhoriaPercentual, 2),
                    ReducaoDiasAtraso = Math.Round(reducaoDias, 2),
                    InterpretacaoImpacto = interpretacao
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar redução de inadimplência");
            throw;
        }
    }

    // ========================================================================
    // RELATÓRIOS HÍBRIDOS (OMNICHANNEL)
    // ========================================================================

    public async Task<List<TempoEnvioPagamentoResponse>> GetTempoEnvioPagamentoAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dados = await (from c in _context.Cobrancas
                              join rc in _context.RegrasCobranca on c.RegraCobrancaId equals rc.Id
                              where c.EmpresaClienteId == empresaClienteId &&
                                    c.DataProcessamento >= dataInicio &&
                                    c.DataProcessamento <= dataFim &&
                                    c.Status == StatusCobranca.Processada
                              select new
                              {
                                  RegraId = rc.Id,
                                  rc.Nome,
                                  CobrancaId = c.Id,
                                  c.PayloadJson,
                                  c.DataProcessamento
                              }).ToListAsync(cancellationToken);

            var cobrancasIds = dados.Select(d => d.CobrancaId).ToList();

            var notificacoes = await _context.HistoricosNotificacao
                .Where(h => cobrancasIds.Contains(h.CobrancaId))
                .Select(h => new
                {
                    h.CobrancaId,
                    h.CanalUtilizado,
                    h.DataEnvio
                })
                .ToListAsync(cancellationToken);

            var faturaIds = dados
                .Select(d => ExtractFaturaIdFromPayload(d.PayloadJson))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id) && f.Status == StatusFatura.Pago)
                .Select(f => new { f.Id, ValorLiquido = f.ValorLiquido.Valor, f.DataPagamento })
                .ToListAsync(cancellationToken);

            var resultado = dados
                .GroupBy(d => new { d.RegraId, d.Nome })
                .Select(g =>
                {
                    var cobrancasIdsGrupo = g.Select(x => x.CobrancaId).ToList();
                    var notificacoesGrupo = notificacoes.Where(n => cobrancasIdsGrupo.Contains(n.CobrancaId)).ToList();
                    var canaisUtilizados = notificacoesGrupo.Select(n => n.CanalUtilizado).Distinct().ToList();

                    var faturaIdsGrupo = g
                        .Select(x => ExtractFaturaIdFromPayload(x.PayloadJson))
                        .Where(id => id != Guid.Empty)
                        .ToList();

                    var faturasGrupo = faturas.Where(f => faturaIdsGrupo.Contains(f.Id)).ToList();

                    var mediaHorasPrimeiro = faturasGrupo.Any()
                        ? faturasGrupo.Average(f =>
                        {
                            var cobranca = g.FirstOrDefault(x => ExtractFaturaIdFromPayload(x.PayloadJson) == f.Id);
                            if (cobranca == null || !f.DataPagamento.HasValue) return 0.0;

                            var primeiroEnvio = notificacoesGrupo
                                .Where(n => n.CobrancaId == cobranca.CobrancaId)
                                .OrderBy(n => n.DataEnvio)
                                .FirstOrDefault();

                            if (primeiroEnvio == null) return 0.0;
                            return (f.DataPagamento.Value - primeiroEnvio.DataEnvio).TotalHours;
                        })
                        : 0;

                    var mediaHorasUltimo = faturasGrupo.Any()
                        ? faturasGrupo.Average(f =>
                        {
                            var cobranca = g.FirstOrDefault(x => ExtractFaturaIdFromPayload(x.PayloadJson) == f.Id);
                            if (cobranca == null || !f.DataPagamento.HasValue) return 0.0;

                            var ultimoEnvio = notificacoesGrupo
                                .Where(n => n.CobrancaId == cobranca.CobrancaId)
                                .OrderByDescending(n => n.DataEnvio)
                                .FirstOrDefault();

                            if (ultimoEnvio == null) return 0.0;
                            return (f.DataPagamento.Value - ultimoEnvio.DataEnvio).TotalHours;
                        })
                        : 0;

                    var mediaEnvios = faturasGrupo.Any()
                        ? faturasGrupo.Average(f =>
                        {
                            var cobranca = g.FirstOrDefault(x => ExtractFaturaIdFromPayload(x.PayloadJson) == f.Id);
                            if (cobranca == null || !f.DataPagamento.HasValue) return 0.0;

                            return (double)notificacoesGrupo.Count(n =>
                                n.CobrancaId == cobranca.CobrancaId &&
                                n.DataEnvio <= f.DataPagamento.Value);
                        })
                        : 0.0;

                    return new TempoEnvioPagamentoResponse
                    {
                        NomeRegua = g.Key.Nome,
                        CanaisUtilizados = canaisUtilizados,
                        QuantidadeCanais = canaisUtilizados.Count,
                        MediaHorasPrimeiroEnvio = Math.Round((decimal)mediaHorasPrimeiro, 2),
                        MediaHorasUltimoEnvio = Math.Round((decimal)mediaHorasUltimo, 2),
                        TotalFaturasPagas = faturasGrupo.Count,
                        TicketMedio = faturasGrupo.Any()
                            ? Math.Round(faturasGrupo.Average(f => f.ValorLiquido), 2)
                            : 0,
                        MediaEnviosAtePagamento = Math.Round((decimal)mediaEnvios, 2)
                    };
                })
                .OrderBy(x => x.MediaHorasPrimeiroEnvio)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tempo envio → pagamento");
            throw;
        }
    }

    public async Task<List<ComparativoOmnichannelResponse>> GetComparativoOmnichannelAsync(
        Guid empresaClienteId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cobrancas = await _context.Cobrancas
                .Where(c => c.EmpresaClienteId == empresaClienteId &&
                           c.DataProcessamento >= dataInicio &&
                           c.DataProcessamento <= dataFim &&
                           c.Status == StatusCobranca.Processada)
                .Select(c => new { c.Id, c.PayloadJson })
                .ToListAsync(cancellationToken);

            var cobrancasIds = cobrancas.Select(c => c.Id).ToList();

            var notificacoes = await _context.HistoricosNotificacao
                .Where(h => cobrancasIds.Contains(h.CobrancaId) && h.Status == StatusNotificacao.Sucesso)
                .Select(h => new
                {
                    h.CobrancaId,
                    h.CanalUtilizado,
                    h.DataEnvio
                })
                .ToListAsync(cancellationToken);

            var faturaIds = cobrancas
                .Select(c => ExtractFaturaIdFromPayload(c.PayloadJson))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var faturas = await _context.Faturas
                .Where(f => faturaIds.Contains(f.Id))
                .Select(f => new { f.Id, f.Status, ValorLiquido = f.ValorLiquido.Valor, f.DataPagamento })
                .ToListAsync(cancellationToken);

            var cobrancasComCanais = cobrancas
                .Select(c =>
                {
                    var canais = notificacoes
                        .Where(n => n.CobrancaId == c.Id)
                        .Select(n => n.CanalUtilizado)
                        .Distinct()
                        .OrderBy(x => (int)x)
                        .ToList();

                    var faturaId = ExtractFaturaIdFromPayload(c.PayloadJson);
                    var fatura = faturas.FirstOrDefault(f => f.Id == faturaId);

                    var primeiroEnvio = notificacoes
                        .Where(n => n.CobrancaId == c.Id)
                        .OrderBy(n => n.DataEnvio)
                        .FirstOrDefault();

                    var horasAtePagamento = 0.0;
                    if (fatura?.Status == StatusFatura.Pago && fatura.DataPagamento.HasValue && primeiroEnvio != null)
                    {
                        horasAtePagamento = (fatura.DataPagamento.Value - primeiroEnvio.DataEnvio).TotalHours;
                    }

                    return new
                    {
                        c.Id,
                        QuantidadeCanais = canais.Count,
                        Canais = canais,
                        FaturaId = faturaId,
                        StatusFatura = fatura?.Status,
                        ValorLiquido = fatura?.ValorLiquido ?? 0,
                        HorasAtePagamento = horasAtePagamento
                    };
                })
                .ToList();

            var resultado = cobrancasComCanais
                .GroupBy(c => new
                {
                    TipoEstrategia = c.QuantidadeCanais switch
                    {
                        1 => "Single-Channel",
                        2 => "Omnichannel (2 canais)",
                        _ => "Omnichannel (3+ canais)"
                    },
                    Canais = string.Join(",", c.Canais.Select(x => (int)x))
                })
                .Select(g =>
                {
                    var totalCobrancas = g.Count();
                    var faturasPagas = g.Count(x => x.StatusFatura == StatusFatura.Pago);
                    var valorRecuperado = g.Where(x => x.StatusFatura == StatusFatura.Pago).Sum(x => x.ValorLiquido);

                    var custoTotal = g.Sum(x => x.Canais.Sum(canal => ObterCustoPorCanal(canal)));

                    var mediaHoras = g.Where(x => x.StatusFatura == StatusFatura.Pago && x.HorasAtePagamento > 0)
                        .DefaultIfEmpty()
                        .Average(x => x?.HorasAtePagamento ?? 0);

                    var roi = custoTotal > 0
                        ? Math.Round((valorRecuperado - custoTotal) / custoTotal * 100, 2)
                        : 0;

                    return new ComparativoOmnichannelResponse
                    {
                        TipoEstrategia = g.Key.TipoEstrategia,
                        CombinacaoCanais = g.First().Canais,
                        TotalCobrancas = totalCobrancas,
                        FaturasPagas = faturasPagas,
                        TaxaConversao = totalCobrancas > 0
                            ? Math.Round((decimal)faturasPagas / totalCobrancas * 100, 2)
                            : 0,
                        ValorRecuperado = valorRecuperado,
                        TicketMedio = faturasPagas > 0
                            ? Math.Round(valorRecuperado / faturasPagas, 2)
                            : 0,
                        MediaHorasConversao = Math.Round((decimal)mediaHoras, 2),
                        CustoTotal = custoTotal,
                        ROI = roi
                    };
                })
                .OrderByDescending(x => x.TaxaConversao)
                .ToList();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar comparativo omnichannel");
            throw;
        }
    }

    // ========================================================================
    // MÉTODOS AUXILIARES
    // ========================================================================

    private static decimal ExtractValorFromPayload(string payloadJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
                return 0;

            var valorMatch = Regex.Match(payloadJson, @"""valor"":\s*(\d+\.?\d*)");
            if (valorMatch.Success && decimal.TryParse(valorMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
            {
                return valor; // Valor já vem no formato correto
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static Guid ExtractFaturaIdFromPayload(string payloadJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
                return Guid.Empty;

            var faturaIdMatch = Regex.Match(payloadJson, @"""faturaId"":\s*""([a-fA-F0-9\-]+)""");
            if (faturaIdMatch.Success && Guid.TryParse(faturaIdMatch.Groups[1].Value, out var faturaId))
            {
                return faturaId;
            }
            return Guid.Empty;
        }
        catch
        {
            return Guid.Empty;
        }
    }

    private static decimal CalcularVariacaoPercentual(decimal valorAtual, decimal valorAnterior)
    {
        if (valorAnterior == 0)
            return valorAtual > 0 ? 100 : 0;

        return Math.Round((valorAtual - valorAnterior) / valorAnterior * 100, 2);
    }

    private static string ClassificarTipoErro(string? mensagemErro)
    {
        if (string.IsNullOrWhiteSpace(mensagemErro))
            return "Outros";

        var mensagem = mensagemErro.ToLower();

        if (mensagem.Contains("timeout"))
            return "Timeout";
        if (mensagem.Contains("invalid email") || mensagem.Contains("email inválido"))
            return "Email Inválido";
        if (mensagem.Contains("bounce"))
            return "Bounce";
        if (mensagem.Contains("blocked") || mensagem.Contains("bloqueado"))
            return "Bloqueado";

        return "Outros";
    }

    private static decimal ObterCustoPorCanal(CanalNotificacao canal)
    {
        return canal switch
        {
            CanalNotificacao.Email => 0.01m,
            CanalNotificacao.SMS => 0.15m,
            CanalNotificacao.WhatsApp => 0.10m,
            _ => 0m
        };
    }

    private static string FormatarTiming(TipoMomento tipoMomento, int valorTempo, UnidadeTempo unidadeTempo)
    {
        var unidade = unidadeTempo switch
        {
            UnidadeTempo.Minutos => valorTempo == 1 ? "minuto" : "minutos",
            UnidadeTempo.Horas => valorTempo == 1 ? "hora" : "horas",
            UnidadeTempo.Dias => valorTempo == 1 ? "dia" : "dias",
            _ => ""
        };

        var momento = tipoMomento switch
        {
            TipoMomento.Antes => "antes",
            TipoMomento.Depois => "depois",
            TipoMomento.Exatamente => "no momento",
            _ => ""
        };

        return $"{valorTempo} {unidade} {momento}";
    }

    private static string ObterNomeDiaSemana(int dia)
    {
        return dia switch
        {
            1 => "Domingo",
            2 => "Segunda",
            3 => "Terça",
            4 => "Quarta",
            5 => "Quinta",
            6 => "Sexta",
            7 => "Sábado",
            _ => "Desconhecido"
        };
    }

    private static string ClassificarPeriodo(int hora)
    {
        return hora switch
        {
            >= 6 and < 12 => "Manhã (6h-12h)",
            >= 12 and < 18 => "Tarde (12h-18h)",
            >= 18 and <= 23 => "Noite (18h-24h)",
            _ => "Madrugada (0h-6h)"
        };
    }

    private MetricasComReguaDto CalcularMetricasComRegua<T>(List<T> faturas) where T : class
    {
        var totalFaturas = faturas.Count;

        // Use reflection to access Status, DataPagamento, DataVencimento properties
        var statusProp = typeof(T).GetProperty("Status");
        var dataPagamentoProp = typeof(T).GetProperty("DataPagamento");
        var dataVencimentoProp = typeof(T).GetProperty("DataVencimento");

        var faturasPagas = faturas.Count(f =>
        {
            var status = statusProp?.GetValue(f);
            return status != null && status.Equals(StatusFatura.Pago);
        });

        var mediaDiasAtraso = faturas
            .Where(f =>
            {
                var status = statusProp?.GetValue(f);
                var dataPagamento = dataPagamentoProp?.GetValue(f) as DateTime?;
                return status != null && status.Equals(StatusFatura.Pago) && dataPagamento != null;
            })
            .Select(f =>
            {
                var dataPagamento = (DateTime?)(dataPagamentoProp?.GetValue(f));
                var dataVencimento = (DateTime)(dataVencimentoProp?.GetValue(f) ?? DateTime.MinValue);
                return (dataPagamento!.Value - dataVencimento).Days;
            })
            .DefaultIfEmpty(0)
            .Average();

        var taxaPagamento = totalFaturas > 0
            ? Math.Round((decimal)faturasPagas / totalFaturas * 100, 2)
            : 0;

        return new MetricasComReguaDto
        {
            TotalFaturas = totalFaturas,
            FaturasPagas = faturasPagas,
            MediaDiasAtraso = Math.Round((decimal)mediaDiasAtraso, 2),
            TaxaPagamento = taxaPagamento
        };
    }

    private MetricasSemReguaDto CalcularMetricasSemRegua<T>(List<T> faturas) where T : class
    {
        var totalFaturas = faturas.Count;

        // Use reflection to access Status, DataPagamento, DataVencimento properties
        var statusProp = typeof(T).GetProperty("Status");
        var dataPagamentoProp = typeof(T).GetProperty("DataPagamento");
        var dataVencimentoProp = typeof(T).GetProperty("DataVencimento");

        var faturasPagas = faturas.Count(f =>
        {
            var status = statusProp?.GetValue(f);
            return status != null && status.Equals(StatusFatura.Pago);
        });

        var mediaDiasAtraso = faturas
            .Where(f =>
            {
                var status = statusProp?.GetValue(f);
                var dataPagamento = dataPagamentoProp?.GetValue(f) as DateTime?;
                return status != null && status.Equals(StatusFatura.Pago) && dataPagamento != null;
            })
            .Select(f =>
            {
                var dataPagamento = (DateTime?)(dataPagamentoProp?.GetValue(f));
                var dataVencimento = (DateTime)(dataVencimentoProp?.GetValue(f) ?? DateTime.MinValue);
                return (dataPagamento!.Value - dataVencimento).Days;
            })
            .DefaultIfEmpty(0)
            .Average();

        var taxaPagamento = totalFaturas > 0
            ? Math.Round((decimal)faturasPagas / totalFaturas * 100, 2)
            : 0;

        return new MetricasSemReguaDto
        {
            TotalFaturas = totalFaturas,
            FaturasPagas = faturasPagas,
            MediaDiasAtraso = Math.Round((decimal)mediaDiasAtraso, 2),
            TaxaPagamento = taxaPagamento
        };
    }
}
