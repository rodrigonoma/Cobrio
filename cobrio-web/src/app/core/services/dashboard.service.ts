import { Injectable } from '@angular/core';
import { Observable, forkJoin, map } from 'rxjs';
import { ApiService } from './api.service';
import { PlanoOfertaService } from './plano-oferta.service';
import { AssinaturaService } from './assinatura.service';

export interface DashboardMetrics {
  totalPlanos: number;
  planosAtivos: number;
  totalAssinaturas: number;
  assinaturasAtivas: number;
  receitaMensal: number;
  taxaConversao: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  constructor(
    private api: ApiService,
    private planoService: PlanoOfertaService,
    private assinaturaService: AssinaturaService
  ) { }

  getMetrics(): Observable<DashboardMetrics> {
    return forkJoin({
      planos: this.planoService.getAll(),
      assinaturas: this.assinaturaService.getAll()
    }).pipe(
      map(({ planos, assinaturas }) => {
        const planosAtivos = planos.filter(p => p.ativo).length;
        const assinaturasAtivas = assinaturas.filter(a =>
          a.status === 'Ativa' || a.status === 'EmTrial'
        ).length;

        // Calcula MRR (Monthly Recurring Revenue)
        const receitaMensal = assinaturas
          .filter(a => a.status === 'Ativa' || a.status === 'EmTrial')
          .reduce((sum, a) => {
            // Normaliza para valor mensal baseado no tipo de ciclo
            let valorMensal = a.valor;
            switch (a.tipoCiclo) {
              case 'Anual':
                valorMensal = a.valor / 12;
                break;
              case 'Semestral':
                valorMensal = a.valor / 6;
                break;
              case 'Trimestral':
                valorMensal = a.valor / 3;
                break;
              default: // Mensal
                valorMensal = a.valor;
            }
            return sum + valorMensal;
          }, 0);

        // Taxa de conversão: (assinaturas ativas / total) * 100
        const taxaConversao = assinaturas.length > 0
          ? (assinaturasAtivas / assinaturas.length) * 100
          : 0;

        return {
          totalPlanos: planos.length,
          planosAtivos: planosAtivos,
          totalAssinaturas: assinaturas.length,
          assinaturasAtivas: assinaturasAtivas,
          receitaMensal: Math.round(receitaMensal),
          taxaConversao: parseFloat(taxaConversao.toFixed(1))
        };
      })
    );
  }
}
