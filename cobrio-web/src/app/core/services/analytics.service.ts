import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

// DTOs que correspondem ao backend
export interface DashboardAnalyticsResponse {
  // KPIs Principais
  valorRecuperadoMesAtual: number;
  taxaConversaoCobranca: number;
  mensagensEnviadasTotal: number;
  mensagensEnviadasUltimos7Dias: number;
  taxaEntregaMedia: number;
  taxaLeituraMedia: number;
  taxaCliqueMedia: number;
  tempoMedioAtePagamentoDias: number;
  mensagensComErro: number;
  percentualErro: number;

  // Performance das Réguas
  performanceReguas: PerformanceRegua[];

  // Efetividade por Canal
  efetividadeCanais: EfetividadeCanal[];

  // Análise Financeira
  analiseFinanceira: AnaliseFinanceira;

  // Engajamento
  engajamento: Engajamento;

  // Status Operacional
  statusOperacional: StatusOperacional;

  // Timeline de Eventos
  timelineEventos: EventoTimeline[];

  // Insights
  insights: Insight[];
}

export interface PerformanceRegua {
  reguaId: string;
  nome: string;
  tipo: string;
  canal: string;
  taxaConversao: number;
  valorRecuperado: number;
  mensagensEnviadas: number;
  pagamentosGerados: number;
}

export interface EfetividadeCanal {
  canal: string;
  entregues: number;
  lidas: number;
  respondidas: number;
  taxaConversao: number;
  custoMensagem: number;
  percentualEntrega: number;
  percentualLeitura: number;
  percentualResposta: number;
}

export interface AnaliseFinanceira {
  totalRecuperado30Dias: number;
  custoTotalDisparos: number;
  roiMedio: number;
  ticketMedioPago: number;
  valorPorDia: ValorRecuperadoPorDia[];
  receitaPorTipo: ReceitaPorTipoRegua[];
}

export interface ValorRecuperadoPorDia {
  data: string;
  valor: number;
}

export interface ReceitaPorTipoRegua {
  tipo: string;
  valor: number;
  percentual: number;
}

export interface Engajamento {
  melhorHorario: string;
  melhorDiaSemana: string;
  canalMaisEngajamento: string;
  heatmapHorarios: HeatmapHorario[];
  clientesMaisEngajados: ClienteEngajado[];
}

export interface HeatmapHorario {
  diaSemana: number;
  hora: number;
  quantidadeEngajamentos: number;
  taxaEngajamento: number;
}

export interface ClienteEngajado {
  nome: string;
  aberturas: number;
  respostas: number;
  cliques: number;
}

export interface StatusOperacional {
  filaEnvio: StatusFila;
  webhook: StatusWebhook;
  provedores: StatusProvedor[];
}

export interface StatusFila {
  status: string;
  ultimaExecucao: string | null;
  mensagensNaFila: number;
}

export interface StatusWebhook {
  status: string;
  ultimaAtualizacao: string | null;
}

export interface StatusProvedor {
  nome: string;
  status: string;
  percentualUtilizado: number;
  limiteTotal: number;
  limiteUtilizado: number;
}

export interface EventoTimeline {
  dataHora: string;
  tipo: string;
  severidade: string;
  descricao: string;
  icone: string;
  valor: number | null;
}

export interface Insight {
  tipo: string;
  titulo: string;
  descricao: string;
  icone: string;
  cor: string;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {

  constructor(private api: ApiService) { }

  /**
   * Obtém todos os dados analíticos do dashboard
   * @param dias Número de dias para análise (padrão: 30)
   * @param tipoRegua Filtro de tipo de régua (opcional)
   * @param canal Filtro de canal (opcional)
   * @returns Observable com os dados do dashboard
   */
  getDashboardAnalytics(
    dias: number = 30,
    tipoRegua: string | null = null,
    canal: string | null = null
  ): Observable<DashboardAnalyticsResponse> {
    const params: any = { dias };
    if (tipoRegua) params.tipoRegua = tipoRegua;
    if (canal) params.canal = canal;

    return this.api.get<DashboardAnalyticsResponse>('Analytics/dashboard', params);
  }
}
