import { Component, OnInit } from '@angular/core';
import { AssinaturaService } from '../../../core/services/assinatura.service';
import { Assinatura, STATUS_ASSINATURA_OPTIONS } from '../../../core/models/assinatura.models';
import { ChartConfiguration, ChartData } from 'chart.js';

interface FinanceiroMetrics {
  recebidos: number;
  pendentes: number;
  vencidos: number;
  total: number;
}

interface TransacaoFinanceira {
  id: string;
  assinaturaId: string;
  assinante: string;
  plano: string;
  valor: number;
  status: 'Pago' | 'Pendente' | 'Vencido' | 'Cancelado';
  dataVencimento: string;
  dataPagamento?: string;
  metodo: string;
}

@Component({
  selector: 'app-financeiro-list',
  templateUrl: './financeiro-list.component.html',
  styleUrls: ['./financeiro-list.component.scss']
})
export class FinanceiroListComponent implements OnInit {
  loading = true;
  assinaturas: Assinatura[] = [];
  transacoes: TransacaoFinanceira[] = [];
  transacoesFiltradas: TransacaoFinanceira[] = [];

  metrics: FinanceiroMetrics = {
    recebidos: 0,
    pendentes: 0,
    vencidos: 0,
    total: 0
  };

  // Filtros
  filtroStatus: string = 'todos';
  filtroPeriodo: string = '30';

  statusOptions = [
    { label: 'Todos', value: 'todos' },
    { label: 'Pagos', value: 'Pago' },
    { label: 'Pendentes', value: 'Pendente' },
    { label: 'Vencidos', value: 'Vencido' },
    { label: 'Cancelados', value: 'Cancelado' }
  ];

  periodoOptions = [
    { label: 'Últimos 7 dias', value: '7' },
    { label: 'Últimos 30 dias', value: '30' },
    { label: 'Últimos 90 dias', value: '90' },
    { label: 'Último ano', value: '365' },
    { label: 'Todos', value: 'todos' }
  ];

  // Gráfico de receita mensal
  receitaChartData: ChartData<'line'> = {
    labels: [],
    datasets: [{
      label: 'Receita',
      data: [],
      borderColor: '#10B981',
      backgroundColor: 'rgba(16, 185, 129, 0.1)',
      tension: 0.4,
      fill: true
    }]
  };

  receitaChartOptions: ChartConfiguration['options'] = {
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

  constructor(
    private assinaturaService: AssinaturaService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;

    this.assinaturaService.getAll().subscribe({
      next: (assinaturas) => {
        this.assinaturas = assinaturas;
        this.processarTransacoes();
        this.calcularMetricas();
        this.prepararGraficoReceita();
        this.aplicarFiltros();
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar dados financeiros', error);
        this.loading = false;
      }
    });
  }

  processarTransacoes(): void {
    // Gera transações fictícias baseadas nas assinaturas
    this.transacoes = this.assinaturas
      .filter(a => a.status === 'Ativa' || a.status === 'Suspensa' || a.status === 'Cancelada')
      .map(assinatura => {
        const hoje = new Date();
        const dataProximaCobranca = assinatura.dataProximaCobranca
          ? new Date(assinatura.dataProximaCobranca)
          : new Date();

        let status: 'Pago' | 'Pendente' | 'Vencido' | 'Cancelado' = 'Pendente';
        let dataPagamento: string | undefined = undefined;

        if (assinatura.status === 'Ativa') {
          const diasAtePagamento = Math.floor((dataProximaCobranca.getTime() - hoje.getTime()) / (1000 * 60 * 60 * 24));

          if (diasAtePagamento < -5) {
            status = 'Vencido';
          } else if (diasAtePagamento < 0) {
            status = 'Pago';
            dataPagamento = new Date(hoje.getTime() - Math.abs(diasAtePagamento) * 24 * 60 * 60 * 1000).toISOString();
          } else {
            status = 'Pendente';
          }
        } else if (assinatura.status === 'Cancelada') {
          status = 'Cancelado';
        }

        return {
          id: assinatura.id,
          assinaturaId: assinatura.id,
          assinante: assinatura.assinanteNome,
          plano: assinatura.planoNome,
          valor: assinatura.valor,
          status: status,
          dataVencimento: assinatura.dataProximaCobranca || assinatura.dataInicio,
          dataPagamento: dataPagamento,
          metodo: 'Cartão de Crédito'
        };
      });
  }

  calcularMetricas(): void {
    const recebidos = this.transacoes
      .filter(t => t.status === 'Pago')
      .reduce((sum, t) => sum + t.valor, 0);

    const pendentes = this.transacoes
      .filter(t => t.status === 'Pendente')
      .reduce((sum, t) => sum + t.valor, 0);

    const vencidos = this.transacoes
      .filter(t => t.status === 'Vencido')
      .reduce((sum, t) => sum + t.valor, 0);

    this.metrics = {
      recebidos: Math.round(recebidos),
      pendentes: Math.round(pendentes),
      vencidos: Math.round(vencidos),
      total: Math.round(recebidos + pendentes + vencidos)
    };
  }

  prepararGraficoReceita(): void {
    // Simula receita dos últimos 6 meses
    const meses = ['Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
    const receitaAtual = this.metrics.recebidos;

    // Simula crescimento de 10% ao mês
    const valores = [];
    let valorBase = receitaAtual / Math.pow(1.1, 5);

    for (let i = 0; i < 6; i++) {
      valores.push(Math.round(valorBase));
      valorBase *= 1.1;
    }

    this.receitaChartData = {
      ...this.receitaChartData,
      labels: meses,
      datasets: [{
        ...this.receitaChartData.datasets[0],
        data: valores
      }]
    };
  }

  aplicarFiltros(): void {
    let filtradas = [...this.transacoes];

    // Filtro por status
    if (this.filtroStatus !== 'todos') {
      filtradas = filtradas.filter(t => t.status === this.filtroStatus);
    }

    // Filtro por período
    if (this.filtroPeriodo !== 'todos') {
      const diasAtras = parseInt(this.filtroPeriodo);
      const dataLimite = new Date();
      dataLimite.setDate(dataLimite.getDate() - diasAtras);

      filtradas = filtradas.filter(t => {
        const dataTransacao = new Date(t.dataVencimento);
        return dataTransacao >= dataLimite;
      });
    }

    this.transacoesFiltradas = filtradas;
  }

  onFiltroChange(): void {
    this.aplicarFiltros();
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
    const severityMap: { [key: string]: string } = {
      'Pago': 'success',
      'Pendente': 'warning',
      'Vencido': 'danger',
      'Cancelado': 'secondary'
    };
    return severityMap[status] || 'info';
  }

  getStatusIcon(status: string): string {
    const iconMap: { [key: string]: string } = {
      'Pago': 'pi-check-circle',
      'Pendente': 'pi-clock',
      'Vencido': 'pi-times-circle',
      'Cancelado': 'pi-ban'
    };
    return iconMap[status] || 'pi-question-circle';
  }

  getTotalFiltrado(): number {
    return this.transacoesFiltradas.reduce((sum, t) => sum + t.valor, 0);
  }

  getTaxaSucesso(): number {
    if (this.transacoes.length === 0) return 0;
    const pagos = this.transacoes.filter(t => t.status === 'Pago').length;
    return (pagos / this.transacoes.length) * 100;
  }

  getTicketMedio(): number {
    if (this.transacoes.length === 0) return 0;
    return this.metrics.total / this.transacoes.length;
  }
}
