import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  RelatoriosAvancadosService,
  DashboardOperacionalResponse,
  ExecucaoReguaResponse,
  CobrancasRecebimentosResponse,
  ValoresPorReguaResponse,
  PagamentosPorAtrasoResponse,
  EntregasFalhasResponse,
  CanalNotificacao
} from '../../core/services/relatorios-avancados.service';

@Component({
  selector: 'app-relatorios-operacionais',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './relatorios-operacionais.component.html',
  styleUrls: ['./relatorios-operacionais.component.css']
})
export class RelatoriosOperacionaisComponent implements OnInit {
  // Filtros
  dataInicio: string = '';
  dataFim: string = '';
  regraCobrancaId?: string;
  canalFiltro?: CanalNotificacao;

  // Loading states
  loadingDashboard = false;
  loadingExecucao = false;
  loadingEntregas = false;
  loadingCobrancas = false;
  loadingValores = false;
  loadingPagamentos = false;

  // Dados
  dashboard?: DashboardOperacionalResponse;
  execucaoReguas: ExecucaoReguaResponse[] = [];
  entregasFalhas?: EntregasFalhasResponse;
  cobrancasRecebimentos: CobrancasRecebimentosResponse[] = [];
  valoresPorRegua: ValoresPorReguaResponse[] = [];
  pagamentosPorAtraso: PagamentosPorAtrasoResponse[] = [];

  // Enum para template
  CanalNotificacao = CanalNotificacao;

  constructor(private relatoriosService: RelatoriosAvancadosService) {
    // Definir período padrão (últimos 30 dias)
    const hoje = new Date();
    const trintaDiasAtras = new Date();
    trintaDiasAtras.setDate(hoje.getDate() - 30);

    this.dataFim = this.formatDateForInput(hoje);
    this.dataInicio = this.formatDateForInput(trintaDiasAtras);
  }

  ngOnInit(): void {
    this.carregarTodosRelatorios();
  }

  carregarTodosRelatorios(): void {
    if (!this.validarDatas()) {
      return;
    }

    this.carregarDashboard();
    this.carregarExecucaoReguas();
    this.carregarEntregasFalhas();
    this.carregarCobrancasRecebimentos();
    this.carregarValoresPorRegua();
    this.carregarPagamentosPorAtraso();
  }

  carregarDashboard(): void {
    this.loadingDashboard = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getDashboardOperacional(inicio, fim, this.regraCobrancaId)
      .subscribe({
        next: (data) => {
          this.dashboard = data;
          this.loadingDashboard = false;
        },
        error: (error) => {
          console.error('Erro ao carregar dashboard:', error);
          this.loadingDashboard = false;
        }
      });
  }

  carregarExecucaoReguas(): void {
    this.loadingExecucao = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getExecucaoReguas(inicio, fim, this.canalFiltro)
      .subscribe({
        next: (data) => {
          this.execucaoReguas = data;
          this.loadingExecucao = false;
        },
        error: (error) => {
          console.error('Erro ao carregar execução de réguas:', error);
          this.loadingExecucao = false;
        }
      });
  }

  carregarEntregasFalhas(): void {
    this.loadingEntregas = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getEntregasFalhas(inicio, fim)
      .subscribe({
        next: (data) => {
          this.entregasFalhas = data;
          this.loadingEntregas = false;
        },
        error: (error) => {
          console.error('Erro ao carregar entregas e falhas:', error);
          this.loadingEntregas = false;
        }
      });
  }

  carregarCobrancasRecebimentos(): void {
    this.loadingCobrancas = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getCobrancasRecebimentos(inicio, fim)
      .subscribe({
        next: (data) => {
          this.cobrancasRecebimentos = data;
          this.loadingCobrancas = false;
        },
        error: (error) => {
          console.error('Erro ao carregar cobranças x recebimentos:', error);
          this.loadingCobrancas = false;
        }
      });
  }

  carregarValoresPorRegua(): void {
    this.loadingValores = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getValoresPorRegua(inicio, fim)
      .subscribe({
        next: (data) => {
          this.valoresPorRegua = data;
          this.loadingValores = false;
        },
        error: (error) => {
          console.error('Erro ao carregar valores por régua:', error);
          this.loadingValores = false;
        }
      });
  }

  carregarPagamentosPorAtraso(): void {
    this.loadingPagamentos = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getPagamentosPorAtraso(inicio, fim)
      .subscribe({
        next: (data) => {
          this.pagamentosPorAtraso = data;
          this.loadingPagamentos = false;
        },
        error: (error) => {
          console.error('Erro ao carregar pagamentos por atraso:', error);
          this.loadingPagamentos = false;
        }
      });
  }

  validarDatas(): boolean {
    if (!this.dataInicio || !this.dataFim) {
      alert('Por favor, selecione as datas de início e fim');
      return false;
    }

    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    if (inicio > fim) {
      alert('A data de início deve ser anterior à data de fim');
      return false;
    }

    const diasDiferenca = (fim.getTime() - inicio.getTime()) / (1000 * 60 * 60 * 24);
    if (diasDiferenca > 365) {
      alert('O período máximo permitido é de 365 dias');
      return false;
    }

    return true;
  }

  formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
  }

  formatCurrency(value: number): string {
    return this.relatoriosService.formatCurrency(value);
  }

  formatPercentage(value: number): string {
    return this.relatoriosService.formatPercentage(value);
  }

  getCanalLabel(canal: CanalNotificacao): string {
    return this.relatoriosService.getCanalLabel(canal);
  }

  getCanalBadgeClass(canal: CanalNotificacao): string {
    switch (canal) {
      case CanalNotificacao.Email:
        return 'badge-email';
      case CanalNotificacao.SMS:
        return 'badge-sms';
      case CanalNotificacao.WhatsApp:
        return 'badge-whatsapp';
      default:
        return 'badge-default';
    }
  }

  limparFiltros(): void {
    const hoje = new Date();
    const trintaDiasAtras = new Date();
    trintaDiasAtras.setDate(hoje.getDate() - 30);

    this.dataFim = this.formatDateForInput(hoje);
    this.dataInicio = this.formatDateForInput(trintaDiasAtras);
    this.regraCobrancaId = undefined;
    this.canalFiltro = undefined;

    this.carregarTodosRelatorios();
  }
}
