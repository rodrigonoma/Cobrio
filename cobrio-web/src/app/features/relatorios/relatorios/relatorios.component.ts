import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { PermissaoService } from '../../../core/services/permissao.service';
import { RelatoriosService, MetricasGeraisResponse, EnvioPorRegraResponse, StatusCobrancaResponse, EvolucaoCobrancaResponse, StatusAssinaturasResponse, HistoricoImportacaoResponse } from '../../../core/services/relatorios.service';
import { MessageService } from 'primeng/api';
import { ChartConfiguration, ChartData } from 'chart.js';
import { forkJoin } from 'rxjs';

interface PeriodoOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-relatorios',
  templateUrl: './relatorios.component.html',
  styleUrls: ['./relatorios.component.scss']
})
export class RelatoriosComponent implements OnInit {
  loading = true;

  // Métricas principais
  metricas: MetricasGeraisResponse = {
    totalCobrado: 0,
    totalEnviadas: 0,
    taxaAbertura: 0,
    emailsAbertos: 0,
    emailsEnviados: 0,
    assinaturasAtivas: 0,
    variacaoTotalCobrado: 0,
    variacaoEnviadas: 0,
    variacaoAssinaturas: 0
  };

  // Dados para gráficos e tabelas
  enviosPorRegra: EnvioPorRegraResponse[] = [];
  statusCobrancas: StatusCobrancaResponse[] = [];
  evolucaoCobrancas: EvolucaoCobrancaResponse[] = [];
  statusAssinaturas: StatusAssinaturasResponse = {
    ativas: 0,
    suspensas: 0,
    canceladas: 0
  };
  historicoImportacoes: HistoricoImportacaoResponse[] = [];

  // Top 5 regras para tabela
  topRegras: any[] = [];

  // Período selecionado
  periodoSelecionado = 'ultimos_30_dias';
  periodosOptions: PeriodoOption[] = [
    { label: 'Hoje', value: 'hoje' },
    { label: 'Últimos 7 dias', value: 'ultimos_7_dias' },
    { label: 'Últimos 30 dias', value: 'ultimos_30_dias' },
    { label: 'Últimos 90 dias', value: 'ultimos_90_dias' },
    { label: 'Este mês', value: 'mes_atual' },
    { label: 'Mês passado', value: 'mes_passado' }
  ];

  // Permissões
  perfilUsuarioString: string = '';
  podeVisualizar = false;
  podeExportar = false;

  // Gráfico de Envios por Regra
  enviosPorRegraChart: ChartData<'bar'> = {
    labels: [],
    datasets: [{
      label: 'Envios',
      data: [],
      backgroundColor: '#3B82F6'
    }]
  };

  enviosPorRegraOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (context) => `Envios: ${context.parsed.y}`
        }
      }
    },
    scales: {
      y: { beginAtZero: true }
    }
  };

  // Gráfico de Status de Cobranças
  statusCobrancasChart: ChartData<'doughnut'> = {
    labels: [],
    datasets: [{
      data: [],
      backgroundColor: ['#10B981', '#F59E0B', '#EF4444']
    }]
  };

  statusCobrancasOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'bottom' },
      tooltip: {
        callbacks: {
          label: (context) => {
            const label = context.label || '';
            const value = context.parsed;
            return `${label}: ${value} (${((value / context.dataset.data.reduce((a: any, b: any) => a + b, 0)) * 100).toFixed(1)}%)`;
          }
        }
      }
    }
  };

  // Gráfico de Evolução de Cobranças
  evolucaoCobrancasChart: ChartData<'line'> = {
    labels: [],
    datasets: [{
      label: 'Valor Cobrado',
      data: [],
      borderColor: '#3B82F6',
      backgroundColor: 'rgba(59, 130, 246, 0.1)',
      tension: 0.4,
      fill: true
    }]
  };

  evolucaoCobrancasOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (context) => this.formatCurrency(context.parsed.y)
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => this.formatCurrency(Number(value))
        }
      }
    }
  };

  // Gráfico de Performance de Emails
  performanceEmailsChart: ChartData<'pie'> = {
    labels: ['Abertos', 'Não Abertos'],
    datasets: [{
      data: [],
      backgroundColor: ['#10B981', '#E5E7EB']
    }]
  };

  performanceEmailsOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'bottom' },
      tooltip: {
        callbacks: {
          label: (context) => {
            const label = context.label || '';
            const value = context.parsed;
            const total = context.dataset.data.reduce((a: any, b: any) => a + b, 0);
            return `${label}: ${value} (${((value / total) * 100).toFixed(1)}%)`;
          }
        }
      }
    }
  };

  constructor(
    private relatoriosService: RelatoriosService,
    private router: Router,
    private authService: AuthService,
    private permissaoService: PermissaoService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.carregarPermissoes();
    this.carregarDados();
  }

  carregarDados(): void {
    this.loading = true;
    const { dataInicio, dataFim } = this.getPeriodoDatas();

    forkJoin({
      metricas: this.relatoriosService.getMetricasGerais(dataInicio, dataFim),
      enviosPorRegra: this.relatoriosService.getEnviosPorRegra(dataInicio, dataFim),
      statusCobrancas: this.relatoriosService.getStatusCobrancas(dataInicio, dataFim),
      evolucaoCobrancas: this.relatoriosService.getEvolucaoCobrancas(dataInicio, dataFim),
      statusAssinaturas: this.relatoriosService.getStatusAssinaturas(),
      historicoImportacoes: this.relatoriosService.getHistoricoImportacoes(dataInicio, dataFim)
    }).subscribe({
      next: (data) => {
        this.metricas = data.metricas;
        this.enviosPorRegra = data.enviosPorRegra;
        this.statusCobrancas = data.statusCobrancas;
        this.evolucaoCobrancas = data.evolucaoCobrancas;
        this.statusAssinaturas = data.statusAssinaturas;
        this.historicoImportacoes = data.historicoImportacoes;

        this.prepareCharts();
        this.prepareTopRegras();
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar relatórios', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar relatórios'
        });
        this.loading = false;
      }
    });
  }

  prepareCharts(): void {
    // Gráfico de Envios por Regra
    this.enviosPorRegraChart = {
      labels: this.enviosPorRegra.map(e => e.nomeRegra),
      datasets: [{
        label: 'Envios',
        data: this.enviosPorRegra.map(e => e.totalEnvios),
        backgroundColor: '#3B82F6'
      }]
    };

    // Gráfico de Status de Cobranças
    this.statusCobrancasChart = {
      labels: this.statusCobrancas.map(s => s.status),
      datasets: [{
        data: this.statusCobrancas.map(s => s.quantidade),
        backgroundColor: ['#10B981', '#F59E0B', '#EF4444']
      }]
    };

    // Gráfico de Evolução de Cobranças
    this.evolucaoCobrancasChart = {
      labels: this.evolucaoCobrancas.map(e => new Date(e.data).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })),
      datasets: [{
        label: 'Valor Cobrado',
        data: this.evolucaoCobrancas.map(e => e.valor),
        borderColor: '#3B82F6',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };

    // Gráfico de Performance de Emails
    const emailsNaoAbertos = this.metricas.emailsEnviados - this.metricas.emailsAbertos;
    this.performanceEmailsChart = {
      labels: ['Abertos', 'Não Abertos'],
      datasets: [{
        data: [this.metricas.emailsAbertos, emailsNaoAbertos],
        backgroundColor: ['#10B981', '#E5E7EB']
      }]
    };
  }

  prepareTopRegras(): void {
    this.topRegras = this.enviosPorRegra.slice(0, 5).map(regra => ({
      nome: regra.nomeRegra,
      envios: regra.totalEnvios,
      valor: regra.valorTotal
    }));
  }

  getPeriodoDatas(): { dataInicio: Date, dataFim: Date } {
    const hoje = new Date();
    const dataFim = new Date(hoje);
    dataFim.setHours(23, 59, 59, 999);
    let dataInicio = new Date(hoje);

    switch (this.periodoSelecionado) {
      case 'hoje':
        dataInicio.setHours(0, 0, 0, 0);
        break;
      case 'ultimos_7_dias':
        dataInicio.setDate(hoje.getDate() - 7);
        dataInicio.setHours(0, 0, 0, 0);
        break;
      case 'ultimos_30_dias':
        dataInicio.setDate(hoje.getDate() - 30);
        dataInicio.setHours(0, 0, 0, 0);
        break;
      case 'ultimos_90_dias':
        dataInicio.setDate(hoje.getDate() - 90);
        dataInicio.setHours(0, 0, 0, 0);
        break;
      case 'mes_atual':
        dataInicio = new Date(hoje.getFullYear(), hoje.getMonth(), 1);
        dataInicio.setHours(0, 0, 0, 0);
        break;
      case 'mes_passado':
        dataInicio = new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1);
        dataInicio.setHours(0, 0, 0, 0);
        dataFim.setDate(0); // Último dia do mês anterior
        dataFim.setHours(23, 59, 59, 999);
        break;
    }

    return { dataInicio, dataFim };
  }

  getPeriodoDescricao(): string {
    const { dataInicio, dataFim } = this.getPeriodoDatas();
    return `${dataInicio.toLocaleDateString('pt-BR')} até ${dataFim.toLocaleDateString('pt-BR')}`;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  }

  getStatusImportacao(importacao: HistoricoImportacaoResponse): string {
    if (importacao.linhasComErro === 0) {
      return 'Sucesso';
    } else if (importacao.linhasProcessadas > 0) {
      return 'Parcial';
    } else {
      return 'Falha';
    }
  }

  getStatusImportacaoSeverity(importacao: HistoricoImportacaoResponse): string {
    if (importacao.linhasComErro === 0) {
      return 'success';
    } else if (importacao.linhasProcessadas > 0) {
      return 'warning';
    } else {
      return 'danger';
    }
  }

  getTaxaSucesso(importacao: HistoricoImportacaoResponse): number {
    if (importacao.totalLinhas === 0) return 0;
    return Math.round((importacao.linhasProcessadas / importacao.totalLinhas) * 100);
  }

  getTaxaSucessoClass(importacao: HistoricoImportacaoResponse): string {
    const taxa = this.getTaxaSucesso(importacao);
    if (taxa >= 90) return 'text-green-600';
    if (taxa >= 70) return 'text-yellow-600';
    return 'text-red-600';
  }

  getPercentualAssinaturas(quantidade: number): number {
    const total = this.statusAssinaturas.ativas +
                  this.statusAssinaturas.suspensas +
                  this.statusAssinaturas.canceladas;
    if (total === 0) return 0;
    return Math.round((quantidade / total) * 100);
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pendente':
        return '#fbbf24'; // yellow
      case 'processada':
        return '#10b981'; // green
      case 'falha':
        return '#ef4444'; // red
      case 'cancelada':
        return '#6b7280'; // gray
      default:
        return '#3b82f6'; // blue
    }
  }

  // Expor Math para o template
  Math = Math;

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
