import { Component, OnInit } from '@angular/core';
import { DashboardService, DashboardMetrics } from '../../core/services/dashboard.service';
import { PlanoOfertaService } from '../../core/services/plano-oferta.service';
import { AssinaturaService } from '../../core/services/assinatura.service';
import { PlanoOferta } from '../../core/models/plano-oferta.models';
import { Assinatura, STATUS_ASSINATURA_OPTIONS } from '../../core/models/assinatura.models';
import { Router } from '@angular/router';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  metrics: DashboardMetrics = {
    totalPlanos: 0,
    planosAtivos: 0,
    totalAssinaturas: 0,
    assinaturasAtivas: 0,
    receitaMensal: 0,
    taxaConversao: 0
  };

  recentPlanos: PlanoOferta[] = [];
  recentAssinaturas: Assinatura[] = [];
  loading = true;
  statusOptions = STATUS_ASSINATURA_OPTIONS;

  // Chart data
  statusChartData: ChartData<'pie'> = {
    labels: [],
    datasets: [{
      data: [],
      backgroundColor: [
        '#10B981', // Success - Ativa
        '#3B82F6', // Info - Em Trial
        '#F59E0B', // Warning - Suspensa
        '#EF4444', // Danger - Cancelada
        '#6B7280', // Secondary - Expirada
        '#F97316'  // Orange - Pendente
      ]
    }]
  };

  statusChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom'
      }
    }
  };

  planosChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [{
      label: 'Assinaturas',
      data: [],
      backgroundColor: '#3B82F6'
    }]
  };

  planosChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1
        }
      }
    }
  };

  constructor(
    private dashboardService: DashboardService,
    private planoService: PlanoOfertaService,
    private assinaturaService: AssinaturaService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;

    // Carrega métricas
    this.dashboardService.getMetrics().subscribe({
      next: (metrics) => {
        this.metrics = metrics;
      },
      error: (error) => {
        console.error('Erro ao carregar métricas', error);
      }
    });

    // Carrega últimos planos
    this.planoService.getAll().subscribe({
      next: (planos) => {
        // Ordena por data de criação (mais recentes primeiro) e pega os 5 primeiros
        this.recentPlanos = planos
          .sort((a, b) => new Date(b.criadoEm).getTime() - new Date(a.criadoEm).getTime())
          .slice(0, 5);
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar planos recentes', error);
        this.loading = false;
      }
    });

    // Carrega últimas assinaturas
    this.assinaturaService.getAll().subscribe({
      next: (assinaturas) => {
        // Ordena por data de criação (mais recentes primeiro) e pega as 5 primeiras
        this.recentAssinaturas = assinaturas
          .sort((a, b) => new Date(b.criadoEm).getTime() - new Date(a.criadoEm).getTime())
          .slice(0, 5);

        // Prepara dados dos gráficos
        this.prepareStatusChart(assinaturas);
        this.preparePlanosChart(assinaturas);
      },
      error: (error) => {
        console.error('Erro ao carregar assinaturas recentes', error);
      }
    });
  }

  prepareStatusChart(assinaturas: Assinatura[]): void {
    // Conta assinaturas por status
    const statusCount = new Map<string, number>();
    assinaturas.forEach(a => {
      const count = statusCount.get(a.status) || 0;
      statusCount.set(a.status, count + 1);
    });

    // Prepara dados para o gráfico
    const labels: string[] = [];
    const data: number[] = [];

    statusCount.forEach((count, status) => {
      const option = this.statusOptions.find(opt => opt.value === status);
      labels.push(option?.label || status);
      data.push(count);
    });

    this.statusChartData = {
      ...this.statusChartData,
      labels: labels,
      datasets: [{
        ...this.statusChartData.datasets[0],
        data: data
      }]
    };
  }

  preparePlanosChart(assinaturas: Assinatura[]): void {
    // Conta assinaturas por plano
    const planoCount = new Map<string, number>();
    assinaturas.forEach(a => {
      const count = planoCount.get(a.planoNome) || 0;
      planoCount.set(a.planoNome, count + 1);
    });

    // Ordena por quantidade e pega os top 5
    const sortedPlanos = Array.from(planoCount.entries())
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5);

    this.planosChartData = {
      ...this.planosChartData,
      labels: sortedPlanos.map(p => p[0]),
      datasets: [{
        ...this.planosChartData.datasets[0],
        data: sortedPlanos.map(p => p[1])
      }]
    };
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value / 100);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('pt-BR');
  }

  getStatusSeverity(status: string): string {
    const option = this.statusOptions.find(opt => opt.value === status);
    return option?.severity || 'info';
  }

  navigateToPlanos(): void {
    this.router.navigate(['/planos']);
  }

  navigateToAssinaturas(): void {
    this.router.navigate(['/assinaturas']);
  }
}
