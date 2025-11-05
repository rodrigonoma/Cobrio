import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AssinaturaService } from '../../../core/services/assinatura.service';
import { PlanoOfertaService } from '../../../core/services/plano-oferta.service';
import { AuthService } from '../../../core/services/auth.service';
import { PermissaoService } from '../../../core/services/permissao.service';
import { MessageService } from 'primeng/api';
import { Assinatura } from '../../../core/models/assinatura.models';
import { PlanoOferta } from '../../../core/models/plano-oferta.models';
import { ChartConfiguration, ChartData } from 'chart.js';

interface MetricaRelatorio {
  titulo: string;
  valor: string;
  variacao: number;
  icon: string;
  iconColor: string;
}

@Component({
  selector: 'app-relatorios',
  templateUrl: './relatorios.component.html',
  styleUrls: ['./relatorios.component.scss']
})
export class RelatoriosComponent implements OnInit {
  loading = true;
  assinaturas: Assinatura[] = [];
  planos: PlanoOferta[] = [];
  Math = Math; // Expose Math for template use

  metricas: MetricaRelatorio[] = [];

  // Permissões
  perfilUsuarioString: string = '';
  podeVisualizar = false;
  podeExportar = false;

  // Gráfico de Evolução de MRR (simulado - últimos 6 meses)
  mrrChartData: ChartData<'line'> = {
    labels: [],
    datasets: [{
      label: 'MRR',
      data: [],
      borderColor: '#3B82F6',
      backgroundColor: 'rgba(59, 130, 246, 0.1)',
      tension: 0.4,
      fill: true
    }]
  };

  mrrChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            return `R$ ${(context.parsed.y / 100).toFixed(2).replace('.', ',')}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => {
            return `R$ ${(Number(value) / 100).toFixed(0)}`;
          }
        }
      }
    }
  };

  // Gráfico de Churn Rate
  churnChartData: ChartData<'line'> = {
    labels: [],
    datasets: [{
      label: 'Churn Rate (%)',
      data: [],
      borderColor: '#EF4444',
      backgroundColor: 'rgba(239, 68, 68, 0.1)',
      tension: 0.4,
      fill: true
    }]
  };

  churnChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            return `${context.parsed.y.toFixed(1)}%`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        max: 100,
        ticks: {
          callback: (value) => {
            return `${value}%`;
          }
        }
      }
    }
  };

  constructor(
    private assinaturaService: AssinaturaService,
    private planoService: PlanoOfertaService,
    private router: Router,
    private authService: AuthService,
    private permissaoService: PermissaoService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.carregarPermissoes();
    this.loadData();
  }

  loadData(): void {
    this.loading = true;

    this.assinaturaService.getAll().subscribe({
      next: (assinaturas) => {
        this.assinaturas = assinaturas;
        this.prepareMetrics();
        this.prepareMrrChart();
        this.prepareChurnChart();
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar dados', error);
        this.loading = false;
      }
    });

    this.planoService.getAll().subscribe({
      next: (planos) => {
        this.planos = planos;
      }
    });
  }

  prepareMetrics(): void {
    // MRR Atual
    const mrrAtual = this.calculateMRR(this.assinaturas.filter(a =>
      a.status === 'Ativa' || a.status === 'EmTrial'
    ));

    // ARR (Annual Recurring Revenue)
    const arr = mrrAtual * 12;

    // Churn Rate do mês
    const canceladas = this.assinaturas.filter(a => a.status === 'Cancelada').length;
    const churnRate = this.assinaturas.length > 0
      ? (canceladas / this.assinaturas.length) * 100
      : 0;

    // Lifetime Value (LTV) estimado
    const ltv = churnRate > 0 ? (mrrAtual / (churnRate / 100)) : 0;

    this.metricas = [
      {
        titulo: 'MRR',
        valor: this.formatCurrency(mrrAtual),
        variacao: 15.3,
        icon: 'pi-chart-line',
        iconColor: 'text-blue-600'
      },
      {
        titulo: 'ARR',
        valor: this.formatCurrency(arr),
        variacao: 12.5,
        icon: 'pi-calendar',
        iconColor: 'text-green-600'
      },
      {
        titulo: 'Churn Rate',
        valor: `${churnRate.toFixed(1)}%`,
        variacao: -2.1,
        icon: 'pi-users',
        iconColor: 'text-red-600'
      },
      {
        titulo: 'LTV Estimado',
        valor: this.formatCurrency(ltv),
        variacao: 8.7,
        icon: 'pi-dollar',
        iconColor: 'text-purple-600'
      }
    ];
  }

  prepareMrrChart(): void {
    // Simula evolução dos últimos 6 meses
    const meses = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun'];
    const mrrAtual = this.calculateMRR(this.assinaturas.filter(a =>
      a.status === 'Ativa' || a.status === 'EmTrial'
    ));

    // Simula crescimento de 15% ao mês (para demonstração)
    const valores = [];
    let valorBase = mrrAtual / 1.15 / 1.15 / 1.15 / 1.15 / 1.15 / 1.15;

    for (let i = 0; i < 6; i++) {
      valores.push(Math.round(valorBase));
      valorBase *= 1.15;
    }

    this.mrrChartData = {
      ...this.mrrChartData,
      labels: meses,
      datasets: [{
        ...this.mrrChartData.datasets[0],
        data: valores
      }]
    };
  }

  prepareChurnChart(): void {
    // Simula churn rate dos últimos 6 meses
    const meses = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun'];

    // Simula valores de churn variando entre 5% e 15%
    const valores = [12.5, 10.2, 8.7, 9.5, 7.3, 6.8];

    this.churnChartData = {
      ...this.churnChartData,
      labels: meses,
      datasets: [{
        ...this.churnChartData.datasets[0],
        data: valores
      }]
    };
  }

  calculateMRR(assinaturas: Assinatura[]): number {
    return assinaturas.reduce((sum, a) => {
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
      }
      return sum + valorMensal;
    }, 0);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value / 100);
  }

  carregarPermissoes(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.perfilUsuarioString = currentUser.perfil;

    // Verificar permissão de visualizar (read)
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'relatorios',
      'read'
    ).subscribe({
      next: (response) => {
        this.podeVisualizar = response.permitido;
      },
      error: () => {
        this.podeVisualizar = false;
      }
    });

    // Verificar permissão de exportar
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'relatorios',
      'export'
    ).subscribe({
      next: (response) => {
        this.podeExportar = response.permitido;
      },
      error: () => {
        this.podeExportar = false;
      }
    });
  }

  exportarPDF(): void {
    if (!this.podeExportar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para exportar relatórios'
      });
      return;
    }

    // TODO: Implementar exportação de PDF
    this.messageService.add({
      severity: 'info',
      summary: 'Em Desenvolvimento',
      detail: 'Funcionalidade de exportação em PDF em desenvolvimento'
    });
  }

  exportarExcel(): void {
    if (!this.podeExportar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para exportar relatórios'
      });
      return;
    }

    // TODO: Implementar exportação de Excel
    this.messageService.add({
      severity: 'info',
      summary: 'Em Desenvolvimento',
      detail: 'Funcionalidade de exportação em Excel em desenvolvimento'
    });
  }
}
