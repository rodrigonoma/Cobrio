import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface MetricasGeraisResponse {
  totalCobrado: number;
  totalEnviadas: number;
  taxaAbertura: number;
  emailsAbertos: number;
  emailsEnviados: number;
  assinaturasAtivas: number;
  variacaoTotalCobrado: number;
  variacaoEnviadas: number;
  variacaoAssinaturas: number;
}

export interface EnvioPorRegraResponse {
  regraId: string;
  nomeRegra: string;
  totalEnvios: number;
  valorTotal: number;
}

export interface StatusCobrancaResponse {
  status: string;
  quantidade: number;
  percentual: number;
}

export interface EvolucaoCobrancaResponse {
  data: Date;
  valor: number;
  quantidade: number;
}

export interface StatusAssinaturasResponse {
  ativas: number;
  suspensas: number;
  canceladas: number;
}

export interface HistoricoImportacaoResponse {
  dataImportacao: Date;
  nomeArquivo: string;
  nomeRegra: string;
  totalLinhas: number;
  linhasProcessadas: number;
  linhasComErro: number;
}

@Injectable({
  providedIn: 'root'
})
export class RelatoriosService {

  constructor(private api: ApiService) { }

  getMetricasGerais(dataInicio: Date, dataFim: Date): Observable<MetricasGeraisResponse> {
    return this.api.get<MetricasGeraisResponse>('Relatorios/metricas-gerais', {
      dataInicio: dataInicio.toISOString(),
      dataFim: dataFim.toISOString()
    });
  }

  getEnviosPorRegra(dataInicio: Date, dataFim: Date): Observable<EnvioPorRegraResponse[]> {
    return this.api.get<EnvioPorRegraResponse[]>('Relatorios/envios-por-regra', {
      dataInicio: dataInicio.toISOString(),
      dataFim: dataFim.toISOString()
    });
  }

  getStatusCobrancas(dataInicio: Date, dataFim: Date): Observable<StatusCobrancaResponse[]> {
    return this.api.get<StatusCobrancaResponse[]>('Relatorios/status-cobrancas', {
      dataInicio: dataInicio.toISOString(),
      dataFim: dataFim.toISOString()
    });
  }

  getEvolucaoCobrancas(dataInicio: Date, dataFim: Date): Observable<EvolucaoCobrancaResponse[]> {
    return this.api.get<EvolucaoCobrancaResponse[]>('Relatorios/evolucao-cobrancas', {
      dataInicio: dataInicio.toISOString(),
      dataFim: dataFim.toISOString()
    });
  }

  getStatusAssinaturas(): Observable<StatusAssinaturasResponse> {
    return this.api.get<StatusAssinaturasResponse>('Relatorios/status-assinaturas');
  }

  getHistoricoImportacoes(dataInicio: Date, dataFim: Date): Observable<HistoricoImportacaoResponse[]> {
    return this.api.get<HistoricoImportacaoResponse[]>('Relatorios/historico-importacoes', {
      dataInicio: dataInicio.toISOString(),
      dataFim: dataFim.toISOString()
    });
  }
}
