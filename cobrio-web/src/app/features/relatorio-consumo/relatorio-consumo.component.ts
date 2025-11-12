import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  RelatoriosAvancadosService,
  DashboardConsumoResponse,
  CanalNotificacao
} from '../../core/services/relatorios-avancados.service';

@Component({
  selector: 'app-relatorio-consumo',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './relatorio-consumo.component.html',
  styleUrls: ['./relatorio-consumo.component.scss']
})
export class RelatorioConsumoComponent implements OnInit {
  // Filtros
  dataInicio: string = '';
  dataFim: string = '';
  canalFiltro?: CanalNotificacao;
  usuarioFiltro?: string;

  // Loading
  loading = false;

  // Dados
  dashboard?: DashboardConsumoResponse;

  // Enum para template
  CanalNotificacao = CanalNotificacao;

  // Opções de filtro
  canaisDisponiveis = [
    { label: 'Todos os Canais', value: undefined },
    { label: 'Email', value: CanalNotificacao.Email },
    { label: 'SMS', value: CanalNotificacao.SMS },
    { label: 'WhatsApp', value: CanalNotificacao.WhatsApp }
  ];

  constructor(private relatoriosService: RelatoriosAvancadosService) {
    // Definir período padrão (mês atual)
    const hoje = new Date();
    const primeiroDiaMes = new Date(hoje.getFullYear(), hoje.getMonth(), 1);

    this.dataFim = this.formatDateForInput(hoje);
    this.dataInicio = this.formatDateForInput(primeiroDiaMes);
  }

  ngOnInit(): void {
    this.carregarDashboard();
  }

  carregarDashboard(): void {
    if (!this.validarDatas()) {
      return;
    }

    this.loading = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getDashboardConsumo(
      inicio,
      fim,
      this.canalFiltro,
      this.usuarioFiltro
    ).subscribe({
      next: (data) => {
        this.dashboard = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar dashboard de consumo', err);
        this.loading = false;
      }
    });
  }

  validarDatas(): boolean {
    if (!this.dataInicio || !this.dataFim) {
      alert('Selecione as datas de início e fim');
      return false;
    }

    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    if (inicio > fim) {
      alert('Data de início deve ser anterior à data de fim');
      return false;
    }

    // Validar período máximo (1 ano)
    const diffDias = Math.floor((fim.getTime() - inicio.getTime()) / (1000 * 60 * 60 * 24));
    if (diffDias > 365) {
      alert('Período máximo permitido: 365 dias');
      return false;
    }

    return true;
  }

  aplicarFiltros(): void {
    this.carregarDashboard();
  }

  limparFiltros(): void {
    const hoje = new Date();
    const primeiroDiaMes = new Date(hoje.getFullYear(), hoje.getMonth(), 1);

    this.dataFim = this.formatDateForInput(hoje);
    this.dataInicio = this.formatDateForInput(primeiroDiaMes);
    this.canalFiltro = undefined;
    this.usuarioFiltro = undefined;

    this.carregarDashboard();
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  getCanalLabel(canal: CanalNotificacao): string {
    return this.relatoriosService.getCanalLabel(canal);
  }

  getCanalColor(canal: CanalNotificacao): string {
    switch (canal) {
      case CanalNotificacao.Email:
        return '#4CAF50';
      case CanalNotificacao.SMS:
        return '#2196F3';
      case CanalNotificacao.WhatsApp:
        return '#25D366';
      default:
        return '#9E9E9E';
    }
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(2)}%`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
  }
}
