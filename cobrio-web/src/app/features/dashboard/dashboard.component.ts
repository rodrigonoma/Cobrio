import { Component, OnInit } from '@angular/core';
import { ChartConfiguration, ChartData } from 'chart.js';
import { AnalyticsService, DashboardAnalyticsResponse } from '../../core/services/analytics.service';

interface KpiCard {
  label: string;
  value: string | number;
  subtitle: string;
  icon: string;
  iconBg: string;
  iconColor: string;
  trend?: string;
  trendPositive?: boolean;
}

interface PerformanceRegua {
  nome: string;
  tipo: string;
  canal: string;
  conversao: number;
  valorRecuperado: number;
  mensagens: number;
}

interface EfetividadeCanal {
  canal: string;
  entregues: number;
  lidas: number;
  respondidas: number;
  conversao: number;
  custo: number;
}

interface EventoTimeline {
  dataHora: Date;
  tipo: string;
  severidade: 'success' | 'info' | 'warning' | 'danger';
  descricao: string;
  icon: string;
  valor?: number;
}

interface StatusProvedor {
  nome: string;
  status: 'success' | 'warning' | 'danger';
  percentualUtilizado: number;
  limite: string;
}

interface Insight {
  tipo: 'insight' | 'recomendacao' | 'alerta';
  titulo: string;
  descricao: string;
  icon: string;
  cor: string;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  loading = false;

  // Filtros
  periodo: string = '30';
  tipoRegua: string = 'todas';
  canal: string = 'todos';

  periodoOptions = [
    { label: 'Últimos 7 dias', value: '7' },
    { label: 'Últimos 30 dias', value: '30' },
    { label: 'Últimos 90 dias', value: '90' },
    { label: 'Customizado', value: 'custom' }
  ];

  tipoReguaOptions = [
    { label: 'Todas', value: 'todas' },
    { label: 'Cobrança', value: 'cobranca' },
    { label: 'Comunicação', value: 'comunicacao' }
  ];

  canalOptions = [
    { label: 'Todos os canais', value: 'todos' },
    { label: 'E-mail', value: 'email' },
    { label: 'WhatsApp', value: 'whatsapp' },
    { label: 'SMS', value: 'sms' }
  ];

  // KPIs Principais
  kpis: KpiCard[] = [];

  // Performance das Réguas
  performanceReguas: PerformanceRegua[] = [];

  // Efetividade por Canal
  efetividadeCanais: EfetividadeCanal[] = [];

  // Gráfico: Valor Recuperado por Dia
  valorRecuperadoChartData: ChartData<'line'> = {
    labels: [],
    datasets: [{
      label: 'Valor Recuperado (R$)',
      data: [],
      borderColor: '#10B981',
      backgroundColor: 'rgba(16, 185, 129, 0.1)',
      fill: true,
      tension: 0.4
    }]
  };

  valorRecuperadoChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (context) => {
            return 'R$ ' + context.parsed.y.toLocaleString('pt-BR');
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => 'R$ ' + value.toLocaleString('pt-BR')
        }
      }
    }
  };

  // Gráfico: Receita por Tipo de Régua
  receitaTipoReguaChartData: ChartData<'doughnut'> = {
    labels: [],
    datasets: [{
      data: [],
      backgroundColor: ['#10B981', '#F59E0B', '#3B82F6']
    }]
  };

  receitaTipoReguaChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'bottom' },
      tooltip: {
        callbacks: {
          label: (context) => {
            const label = context.label || '';
            const value = context.parsed;
            const total = context.dataset.data.reduce((a: any, b: any) => a + b, 0) as number;
            const percentage = ((value / total) * 100).toFixed(1);
            return `${label}: R$ ${value.toLocaleString('pt-BR')} (${percentage}%)`;
          }
        }
      }
    }
  };

  // Gráfico: Performance por Régua
  performanceReguasChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [{
      label: 'Conversão (%)',
      data: [],
      backgroundColor: '#3B82F6'
    }]
  };

  performanceReguasChartOptions: ChartConfiguration['options'] = {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false }
    },
    scales: {
      x: {
        beginAtZero: true,
        max: 50,
        ticks: {
          callback: (value) => value + '%'
        }
      }
    }
  };

  // Gráfico: Efetividade por Canal
  efetividadeCanalChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Entregues',
        data: [],
        backgroundColor: '#3B82F6'
      },
      {
        label: 'Lidas',
        data: [],
        backgroundColor: '#10B981'
      },
      {
        label: 'Respondidas',
        data: [],
        backgroundColor: '#F59E0B'
      }
    ]
  };

  efetividadeCanalChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'bottom' }
    },
    scales: {
      y: { beginAtZero: true }
    }
  };

  // Timeline de Eventos
  eventosTimeline: EventoTimeline[] = [];

  // Status Operacional
  statusProvedores: StatusProvedor[] = [];
  statusFilaEnvio: any = {
    status: 'OK',
    ultimaExecucao: null,
    mensagensNaFila: 0
  };
  statusWebhook: any = {
    status: 'Ativo',
    ultimaAtualizacao: null
  };

  // Insights
  insights: Insight[] = [];

  // Dados de análise financeira
  analiseFinanceira: any = {
    totalRecuperado30Dias: 0,
    custoTotalDisparos: 0,
    roiMedio: 0,
    ticketMedioPago: 0
  };

  // Dados de engajamento
  engajamento: any = {
    melhorHorario: '',
    melhorDiaSemana: '',
    canalMaisEngajamento: ''
  };

  constructor(private analyticsService: AnalyticsService) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;

    const dias = parseInt(this.periodo);
    const tipoReguaFiltro = this.tipoRegua !== 'todas' ? this.tipoRegua : null;
    const canalFiltro = this.canal !== 'todos' ? this.canal : null;

    this.analyticsService.getDashboardAnalytics(dias, tipoReguaFiltro, canalFiltro).subscribe({
      next: (data) => {
        this.processarDadosAnalytics(data);
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar analytics:', error);
        this.loading = false;
        // Mantém dados mock em caso de erro
      }
    });
  }

  private processarDadosAnalytics(data: DashboardAnalyticsResponse): void {
    // Atualizar KPIs
    this.kpis = [
      {
        label: 'Valor Recuperado',
        value: this.formatCurrency(data.valorRecuperadoMesAtual),
        subtitle: 'Mês atual',
        icon: 'pi-dollar',
        iconBg: 'bg-green-100',
        iconColor: 'text-green-600',
        trend: '+12,5%',
        trendPositive: true
      },
      {
        label: 'Taxa de Conversão',
        value: `${data.taxaConversaoCobranca.toFixed(1)}%`,
        subtitle: 'Cobranças pagas',
        icon: 'pi-chart-line',
        iconBg: 'bg-blue-100',
        iconColor: 'text-blue-600',
        trend: '+3,2%',
        trendPositive: true
      },
      {
        label: 'Mensagens Enviadas',
        value: data.mensagensEnviadasTotal.toLocaleString('pt-BR'),
        subtitle: `${data.mensagensEnviadasUltimos7Dias} últimos 7 dias`,
        icon: 'pi-send',
        iconBg: 'bg-purple-100',
        iconColor: 'text-purple-600'
      },
      {
        label: 'Taxa de Entrega',
        value: `${data.taxaEntregaMedia.toFixed(1)}%`,
        subtitle: 'Mensagens entregues',
        icon: 'pi-check-circle',
        iconBg: 'bg-green-100',
        iconColor: 'text-green-600'
      },
      {
        label: 'Taxa de Leitura',
        value: `${data.taxaLeituraMedia.toFixed(1)}%`,
        subtitle: 'Mensagens lidas',
        icon: 'pi-eye',
        iconBg: 'bg-blue-100',
        iconColor: 'text-blue-600'
      },
      {
        label: 'Tempo Médio',
        value: `${data.tempoMedioAtePagamentoDias.toFixed(1)} dias`,
        subtitle: 'Até pagamento',
        icon: 'pi-clock',
        iconBg: 'bg-yellow-100',
        iconColor: 'text-yellow-600'
      },
      {
        label: 'Mensagens com Erro',
        value: data.mensagensComErro.toLocaleString('pt-BR'),
        subtitle: `${data.percentualErro.toFixed(1)}% do total`,
        icon: 'pi-exclamation-triangle',
        iconBg: data.percentualErro > 5 ? 'bg-red-100' : 'bg-orange-100',
        iconColor: data.percentualErro > 5 ? 'text-red-600' : 'text-orange-600'
      },
      {
        label: 'Taxa de Clique',
        value: `${data.taxaCliqueMedia.toFixed(1)}%`,
        subtitle: 'Ações em mensagens',
        icon: 'pi-external-link',
        iconBg: 'bg-indigo-100',
        iconColor: 'text-indigo-600'
      }
    ];

    // Atualizar Performance das Réguas
    this.performanceReguas = data.performanceReguas.map(r => ({
      nome: r.nome,
      tipo: r.tipo,
      canal: r.canal,
      conversao: r.taxaConversao,
      valorRecuperado: r.valorRecuperado,
      mensagens: r.mensagensEnviadas
    }));

    // Atualizar Efetividade de Canais
    this.efetividadeCanais = data.efetividadeCanais.map(c => ({
      canal: c.canal,
      entregues: c.entregues,
      lidas: c.lidas,
      respondidas: c.respondidas,
      conversao: c.taxaConversao,
      custo: c.custoMensagem
    }));

    // Atualizar Status de Provedores
    this.statusProvedores = data.statusOperacional.provedores.map(p => ({
      nome: p.nome,
      status: p.status === 'warning' ? 'warning' : p.status === 'OK' ? 'success' : 'danger',
      percentualUtilizado: p.percentualUtilizado,
      limite: `${p.limiteUtilizado.toLocaleString('pt-BR')} / ${p.limiteTotal.toLocaleString('pt-BR')}`
    }));

    // Atualizar Status da Fila de Envio
    this.statusFilaEnvio = {
      status: data.statusOperacional.filaEnvio.status,
      ultimaExecucao: data.statusOperacional.filaEnvio.ultimaExecucao,
      mensagensNaFila: data.statusOperacional.filaEnvio.mensagensNaFila
    };

    // Atualizar Status do Webhook
    this.statusWebhook = {
      status: data.statusOperacional.webhook.status,
      ultimaAtualizacao: data.statusOperacional.webhook.ultimaAtualizacao
    };

    // Atualizar Timeline
    this.eventosTimeline = data.timelineEventos.map(e => ({
      dataHora: new Date(e.dataHora),
      tipo: e.tipo,
      severidade: e.severidade as 'success' | 'info' | 'warning' | 'danger',
      descricao: e.descricao,
      icon: e.icone,
      valor: e.valor ?? undefined
    }));

    // Atualizar Insights
    this.insights = data.insights.map(i => ({
      tipo: i.tipo.toLowerCase() as 'insight' | 'recomendacao' | 'alerta',
      titulo: i.titulo,
      descricao: i.descricao,
      icon: i.icone,
      cor: i.cor
    }));

    // Atualizar Análise Financeira
    this.analiseFinanceira = {
      totalRecuperado30Dias: data.analiseFinanceira.totalRecuperado30Dias,
      custoTotalDisparos: data.analiseFinanceira.custoTotalDisparos,
      roiMedio: data.analiseFinanceira.roiMedio,
      ticketMedioPago: data.analiseFinanceira.ticketMedioPago
    };

    // Atualizar Engajamento
    this.engajamento = {
      melhorHorario: data.engajamento.melhorHorario,
      melhorDiaSemana: data.engajamento.melhorDiaSemana,
      canalMaisEngajamento: data.engajamento.canalMaisEngajamento
    };

    // Atualizar gráficos
    this.atualizarGraficos(data);
  }

  private atualizarGraficos(data: DashboardAnalyticsResponse): void {
    // Atualizar gráfico de Performance das Réguas
    this.performanceReguasChartData = {
      labels: data.performanceReguas.map(r => r.nome),
      datasets: [{
        label: 'Taxa de Conversão (%)',
        data: data.performanceReguas.map(r => r.taxaConversao),
        backgroundColor: 'rgba(59, 130, 246, 0.8)',
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 1
      }]
    };

    // Atualizar gráfico de Efetividade por Canal
    this.efetividadeCanalChartData = {
      labels: data.efetividadeCanais.map(c => c.canal),
      datasets: [
        {
          label: 'Entregues',
          data: data.efetividadeCanais.map(c => c.percentualEntrega),
          backgroundColor: 'rgba(34, 197, 94, 0.8)'
        },
        {
          label: 'Lidas',
          data: data.efetividadeCanais.map(c => c.percentualLeitura),
          backgroundColor: 'rgba(59, 130, 246, 0.8)'
        },
        {
          label: 'Respondidas',
          data: data.efetividadeCanais.map(c => c.percentualResposta),
          backgroundColor: 'rgba(168, 85, 247, 0.8)'
        }
      ]
    };

    // Atualizar gráfico de Valor Recuperado por Dia
    this.valorRecuperadoChartData = {
      labels: data.analiseFinanceira.valorPorDia.map(v => new Date(v.data).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })),
      datasets: [{
        label: 'Valor Recuperado (R$)',
        data: data.analiseFinanceira.valorPorDia.map(v => v.valor),
        borderColor: '#10B981',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        fill: true,
        tension: 0.4
      }]
    };

    // Atualizar gráfico de Receita por Tipo
    this.receitaTipoReguaChartData = {
      labels: data.analiseFinanceira.receitaPorTipo.map(r => r.tipo),
      datasets: [{
        data: data.analiseFinanceira.receitaPorTipo.map(r => r.valor),
        backgroundColor: [
          'rgba(59, 130, 246, 0.8)',
          'rgba(16, 185, 129, 0.8)',
          'rgba(251, 146, 60, 0.8)'
        ]
      }]
    };
  }

  aplicarFiltros(): void {
    console.log('Filtros aplicados:', { periodo: this.periodo, tipoRegua: this.tipoRegua, canal: this.canal });
    this.loadDashboard();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  }

  formatDateTime(date: Date | string): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    }).format(dateObj);
  }

  getTempoDecorrido(date: Date): string {
    const agora = new Date();
    const diff = agora.getTime() - date.getTime();
    const minutos = Math.floor(diff / 60000);

    if (minutos < 1) return 'Agora';
    if (minutos < 60) return `${minutos}min atrás`;

    const horas = Math.floor(minutos / 60);
    if (horas < 24) return `${horas}h atrás`;

    const dias = Math.floor(horas / 24);
    return `${dias}d atrás`;
  }

  getStatusSeverityClass(status: 'success' | 'warning' | 'danger'): string {
    const classes: Record<string, string> = {
      'success': 'bg-green-50 border-green-200',
      'warning': 'bg-yellow-50 border-yellow-200',
      'danger': 'bg-red-50 border-red-200'
    };
    return classes[status] || 'bg-gray-50 border-gray-200';
  }

  getStatusIconClass(status: 'success' | 'warning' | 'danger'): string {
    const classes: Record<string, string> = {
      'success': 'text-green-600',
      'warning': 'text-yellow-600',
      'danger': 'text-red-600'
    };
    return classes[status] || 'text-gray-600';
  }

  getInsightColorClass(cor: string): string {
    const classes: Record<string, string> = {
      'blue': 'bg-blue-50 border-blue-200',
      'green': 'bg-green-50 border-green-200',
      'purple': 'bg-purple-50 border-purple-200',
      'orange': 'bg-orange-50 border-orange-200'
    };
    return classes[cor] || 'bg-gray-50 border-gray-200';
  }

  getInsightIconColor(cor: string): string {
    const classes: Record<string, string> = {
      'blue': 'text-blue-600',
      'green': 'text-green-600',
      'purple': 'text-purple-600',
      'orange': 'text-orange-600'
    };
    return classes[cor] || 'text-gray-600';
  }
}
