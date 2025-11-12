import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ============================================================================
// INTERFACES - RELATÓRIOS OPERACIONAIS
// ============================================================================

export interface DashboardOperacionalResponse {
  totalCobrancas: number;
  cobrancasProcessadas: number;
  cobrancasFalhas: number;
  cobrancasPendentes: number;
  valorTotal: number;
  valorProcessado: number;
  mediaTentativas: number;
  cobrancasRetentadas: number;
  variacaoProcessadas: number;
  taxaSucesso: number;
}

export interface ExecucaoReguaResponse {
  data: string;
  canal: CanalNotificacao;
  nomeRegra: string;
  regraId: string;
  totalEnvios: number;
  sucessos: number;
  falhas: number;
  taxaSucesso: number;
  cobrancasProcessadas: number;
  cobrancasFalhas: number;
}

export interface EntregasFalhasResponse {
  falhasPorTipo: FalhasPorTipoResponse[];
  pendenciasPorTempo: PendenciasPorTempoResponse[];
}

export interface FalhasPorTipoResponse {
  data: string;
  tipoErro: string;
  quantidade: number;
  regrasAfetadas: string[];
}

export interface PendenciasPorTempoResponse {
  faixaTempo: string;
  quantidade: number;
  valorTotal: number;
}

export interface CobrancasRecebimentosResponse {
  data: string;
  totalCobrancasEnviadas: number;
  valorCobrado: number;
  totalFaturasGeradas: number;
  faturasPagas: number;
  valorRecebido: number;
  taxaRecuperacao: number;
  mediaHorasAtePagamento: number;
  faturasVencidas: number;
}

export interface ValoresPorReguaResponse {
  regraId: string;
  nomeRegra: string;
  canal: CanalNotificacao;
  descricaoTiming: string;
  totalCobrancas: number;
  valorCobrado: number;
  faturasPagas: number;
  valorRecuperado: number;
  taxaRecuperacao: number;
  custoEstimado: number;
  roi: number;
}

export interface PagamentosPorAtrasoResponse {
  faixaAtraso: string;
  quantidadeFaturas: number;
  valorTotal: number;
  ticketMedio: number;
  mediaCobrancasEnviadas: number;
  mediaDiasAteRecuperacao: number;
}

// ============================================================================
// INTERFACES - RELATÓRIOS GERENCIAIS
// ============================================================================

export interface ConversaoPorCanalResponse {
  nomeRegra: string;
  canal: CanalNotificacao;
  funil: FunilConversaoDto;
  taxaProcessamento: number;
  taxaConversaoFinal: number;
  valorRecuperado: number;
  ticketMedio: number;
  mediaHorasConversao: number;
}

export interface FunilConversaoDto {
  totalEnvios: number;
  totalCobrancas: number;
  cobrancasProcessadas: number;
  faturasPagas: number;
}

export interface ROIReguasResponse {
  periodo: string;
  regraId: string;
  nomeRegra: string;
  canal: CanalNotificacao;
  totalEnvios: number;
  custoTotal: number;
  receitaRecuperada: number;
  roi: number;
  lucroLiquido: number;
}

export interface EvolucaoMensalResponse {
  periodo: string;
  totalCobrancas: number;
  cobrancasProcessadas: number;
  valorCobrado: number;
  faturasPagas: number;
  valorRecebido: number;
  taxaSucesso: number;
  taxaConversao: number;
  faturasInadimplentes: number;
  breakdownCanal: BreakdownCanalDto;
}

export interface BreakdownCanalDto {
  enviosEmail: number;
  enviosSMS: number;
  enviosWhatsApp: number;
}

export interface MelhorHorarioEnvioResponse {
  porDiaSemana: PerformancePorDiaResponse[];
  porHoraDia: PerformancePorHoraResponse[];
  recomendacao: RecomendacaoEnvioDto;
}

export interface PerformancePorDiaResponse {
  diaSemana: number;
  nomeDia: string;
  totalCobrancas: number;
  faturasPagas: number;
  taxaConversao: number;
  mediaHorasConversao: number;
}

export interface PerformancePorHoraResponse {
  hora: number;
  periodo: string;
  totalCobrancas: number;
  faturasPagas: number;
  taxaConversao: number;
  mediaHorasConversao: number;
}

export interface RecomendacaoEnvioDto {
  melhorDia: string;
  melhorPeriodo: string;
  taxaConversaoEsperada: number;
}

export interface ReducaoInadimplenciaResponse {
  comRegua: MetricasComReguaDto;
  semRegua: MetricasSemReguaDto;
  impacto: ImpactoReguaDto;
}

export interface MetricasComReguaDto {
  totalFaturas: number;
  faturasPagas: number;
  mediaDiasAtraso: number;
  taxaPagamento: number;
}

export interface MetricasSemReguaDto {
  totalFaturas: number;
  faturasPagas: number;
  mediaDiasAtraso: number;
  taxaPagamento: number;
}

export interface ImpactoReguaDto {
  pontoPercentualMelhoria: number;
  reducaoDiasAtraso: number;
  interpretacaoImpacto: string;
}

// ============================================================================
// INTERFACES - RELATÓRIOS HÍBRIDOS (OMNICHANNEL)
// ============================================================================

export interface TempoEnvioPagamentoResponse {
  nomeRegua: string;
  canaisUtilizados: CanalNotificacao[];
  quantidadeCanais: number;
  mediaHorasPrimeiroEnvio: number;
  mediaHorasUltimoEnvio: number;
  totalFaturasPagas: number;
  ticketMedio: number;
  mediaEnviosAtePagamento: number;
  ehOmnichannel: boolean;
}

export interface ComparativoOmnichannelResponse {
  tipoEstrategia: string;
  combinacaoCanais: CanalNotificacao[];
  totalCobrancas: number;
  faturasPagas: number;
  taxaConversao: number;
  valorRecuperado: number;
  ticketMedio: number;
  mediaHorasConversao: number;
  custoTotal: number;
  roi: number;
}

// ============================================================================
// INTERFACES - RELATÓRIO DE CONSUMO
// ============================================================================

export interface DashboardConsumoResponse {
  dataInicio: string;
  dataFim: string;
  totais: ConsumoTotaisResponse;
  consumoPorCanal: ConsumoPorCanalResponse[];
  consumoPorUsuario: ConsumoPorUsuarioResponse[];
  consumoPorRegua: ConsumoPorReguaResponse[];
  evolucaoTemporal: ConsumoTemporalResponse[];
}

export interface ConsumoTotaisResponse {
  totalEnvios: number;
  totalEmails: number;
  totalSMS: number;
  totalWhatsApp: number;
  mediaEnviosPorDia: number;
}

export interface ConsumoPorCanalResponse {
  canal: CanalNotificacao;
  nomeCanal: string;
  totalEnvios: number;
  sucessos: number;
  falhas: number;
  taxaSucesso: number;
  percentualDoTotal: number;
}

export interface ConsumoPorUsuarioResponse {
  usuarioId?: string;
  nomeUsuario: string;
  totalEnvios: number;
  enviosEmail: number;
  enviosSMS: number;
  enviosWhatsApp: number;
  percentualDoTotal: number;
}

export interface ConsumoPorReguaResponse {
  reguaId: string;
  nomeRegua: string;
  canal: CanalNotificacao;
  totalEnvios: number;
  percentualDoTotal: number;
}

export interface ConsumoTemporalResponse {
  data: string;
  totalEnvios: number;
  enviosEmail: number;
  enviosSMS: number;
  enviosWhatsApp: number;
}

// ============================================================================
// ENUMS
// ============================================================================

export enum CanalNotificacao {
  Email = 1,
  SMS = 2,
  WhatsApp = 3,
  Push = 4
}

// ============================================================================
// FILTROS
// ============================================================================

export interface FiltrosRelatoriosAvancados {
  dataInicio: Date;
  dataFim: Date;
  regraCobrancaId?: string;
  canal?: CanalNotificacao;
}

@Injectable({
  providedIn: 'root'
})
export class RelatoriosAvancadosService {
  private apiUrl = `${environment.apiUrl}/RelatoriosAvancados`;

  constructor(private http: HttpClient) {}

  // ========================================================================
  // RELATÓRIOS OPERACIONAIS
  // ========================================================================

  getDashboardOperacional(
    dataInicio: Date,
    dataFim: Date,
    regraCobrancaId?: string
  ): Observable<DashboardOperacionalResponse> {
    let params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    if (regraCobrancaId) {
      params = params.set('regraCobrancaId', regraCobrancaId);
    }

    return this.http.get<DashboardOperacionalResponse>(
      `${this.apiUrl}/dashboard-operacional`,
      { params }
    );
  }

  getExecucaoReguas(
    dataInicio: Date,
    dataFim: Date,
    canal?: CanalNotificacao
  ): Observable<ExecucaoReguaResponse[]> {
    let params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    if (canal !== undefined) {
      params = params.set('canal', canal.toString());
    }

    return this.http.get<ExecucaoReguaResponse[]>(
      `${this.apiUrl}/execucao-reguas`,
      { params }
    );
  }

  getEntregasFalhas(
    dataInicio: Date,
    dataFim: Date
  ): Observable<EntregasFalhasResponse> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<EntregasFalhasResponse>(
      `${this.apiUrl}/entregas-falhas`,
      { params }
    );
  }

  getCobrancasRecebimentos(
    dataInicio: Date,
    dataFim: Date
  ): Observable<CobrancasRecebimentosResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<CobrancasRecebimentosResponse[]>(
      `${this.apiUrl}/cobrancas-recebimentos`,
      { params }
    );
  }

  getValoresPorRegua(
    dataInicio: Date,
    dataFim: Date
  ): Observable<ValoresPorReguaResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<ValoresPorReguaResponse[]>(
      `${this.apiUrl}/valores-por-regua`,
      { params }
    );
  }

  getPagamentosPorAtraso(
    dataInicio: Date,
    dataFim: Date
  ): Observable<PagamentosPorAtrasoResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<PagamentosPorAtrasoResponse[]>(
      `${this.apiUrl}/pagamentos-por-atraso`,
      { params }
    );
  }

  // ========================================================================
  // RELATÓRIOS GERENCIAIS
  // ========================================================================

  getConversaoPorCanal(
    dataInicio: Date,
    dataFim: Date
  ): Observable<ConversaoPorCanalResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<ConversaoPorCanalResponse[]>(
      `${this.apiUrl}/conversao-por-canal`,
      { params }
    );
  }

  getROIReguas(
    dataInicio: Date,
    dataFim: Date
  ): Observable<ROIReguasResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<ROIReguasResponse[]>(
      `${this.apiUrl}/roi-reguas`,
      { params }
    );
  }

  getEvolucaoMensal(
    dataInicio: Date,
    dataFim: Date
  ): Observable<EvolucaoMensalResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<EvolucaoMensalResponse[]>(
      `${this.apiUrl}/evolucao-mensal`,
      { params }
    );
  }

  getMelhorHorarioEnvio(
    dataInicio: Date,
    dataFim: Date
  ): Observable<MelhorHorarioEnvioResponse> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<MelhorHorarioEnvioResponse>(
      `${this.apiUrl}/melhor-horario-envio`,
      { params }
    );
  }

  getReducaoInadimplencia(
    dataInicio: Date,
    dataFim: Date
  ): Observable<ReducaoInadimplenciaResponse> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<ReducaoInadimplenciaResponse>(
      `${this.apiUrl}/reducao-inadimplencia`,
      { params }
    );
  }

  // ========================================================================
  // RELATÓRIOS HÍBRIDOS (OMNICHANNEL)
  // ========================================================================

  getTempoEnvioPagamento(
    dataInicio: Date,
    dataFim: Date
  ): Observable<TempoEnvioPagamentoResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<TempoEnvioPagamentoResponse[]>(
      `${this.apiUrl}/tempo-envio-pagamento`,
      { params }
    );
  }

  getComparativoOmnichannel(
    dataInicio: Date,
    dataFim: Date
  ): Observable<ComparativoOmnichannelResponse[]> {
    const params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    return this.http.get<ComparativoOmnichannelResponse[]>(
      `${this.apiUrl}/comparativo-omnichannel`,
      { params }
    );
  }

  // ========================================================================
  // RELATÓRIO DE CONSUMO
  // ========================================================================

  getDashboardConsumo(
    dataInicio: Date,
    dataFim: Date,
    canal?: CanalNotificacao,
    usuarioId?: string
  ): Observable<DashboardConsumoResponse> {
    let params = new HttpParams()
      .set('dataInicio', dataInicio.toISOString())
      .set('dataFim', dataFim.toISOString());

    if (canal !== undefined) {
      params = params.set('canal', canal.toString());
    }

    if (usuarioId) {
      params = params.set('usuarioId', usuarioId);
    }

    return this.http.get<DashboardConsumoResponse>(
      `${this.apiUrl}/dashboard-consumo`,
      { params }
    );
  }

  // ========================================================================
  // HELPERS
  // ========================================================================

  getCanalLabel(canal: CanalNotificacao): string {
    switch (canal) {
      case CanalNotificacao.Email:
        return 'Email';
      case CanalNotificacao.SMS:
        return 'SMS';
      case CanalNotificacao.WhatsApp:
        return 'WhatsApp';
      case CanalNotificacao.Push:
        return 'Push';
      default:
        return 'Desconhecido';
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(2)}%`;
  }
}
