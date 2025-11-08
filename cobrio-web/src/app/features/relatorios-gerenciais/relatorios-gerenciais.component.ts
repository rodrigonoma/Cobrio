import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  RelatoriosAvancadosService,
  ConversaoPorCanalResponse,
  ROIReguasResponse,
  EvolucaoMensalResponse,
  MelhorHorarioEnvioResponse,
  ReducaoInadimplenciaResponse,
  TempoEnvioPagamentoResponse,
  ComparativoOmnichannelResponse,
  CanalNotificacao
} from '../../core/services/relatorios-avancados.service';

@Component({
  selector: 'app-relatorios-gerenciais',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './relatorios-gerenciais.component.html',
  styleUrls: ['./relatorios-gerenciais.component.css']
})
export class RelatoriosGerenciaisComponent implements OnInit {
  // Filtros
  dataInicio: string = '';
  dataFim: string = '';

  // Loading states
  loadingConversao = false;
  loadingROI = false;
  loadingEvolucao = false;
  loadingHorarios = false;
  loadingReducao = false;
  loadingTempo = false;
  loadingOmnichannel = false;

  // Dados
  conversaoPorCanal: ConversaoPorCanalResponse[] = [];
  roiReguas: ROIReguasResponse[] = [];
  evolucaoMensal: EvolucaoMensalResponse[] = [];
  melhorHorario?: MelhorHorarioEnvioResponse;
  reducaoInadimplencia?: ReducaoInadimplenciaResponse;
  tempoEnvioPagamento: TempoEnvioPagamentoResponse[] = [];
  comparativoOmnichannel: ComparativoOmnichannelResponse[] = [];

  // Enum para template
  CanalNotificacao = CanalNotificacao;

  constructor(private relatoriosService: RelatoriosAvancadosService) {
    // Definir período padrão (últimos 90 dias para análise gerencial)
    const hoje = new Date();
    const noventaDiasAtras = new Date();
    noventaDiasAtras.setDate(hoje.getDate() - 90);

    this.dataFim = this.formatDateForInput(hoje);
    this.dataInicio = this.formatDateForInput(noventaDiasAtras);
  }

  ngOnInit(): void {
    this.carregarTodosRelatorios();
  }

  carregarTodosRelatorios(): void {
    if (!this.validarDatas()) {
      return;
    }

    this.carregarConversaoPorCanal();
    this.carregarROIReguas();
    this.carregarEvolucaoMensal();
    this.carregarMelhorHorarioEnvio();
    this.carregarReducaoInadimplencia();
    this.carregarTempoEnvioPagamento();
    this.carregarComparativoOmnichannel();
  }

  carregarConversaoPorCanal(): void {
    this.loadingConversao = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getConversaoPorCanal(inicio, fim)
      .subscribe({
        next: (data) => {
          this.conversaoPorCanal = data;
          this.loadingConversao = false;
        },
        error: (error) => {
          console.error('Erro ao carregar conversão por canal:', error);
          this.loadingConversao = false;
        }
      });
  }

  carregarROIReguas(): void {
    this.loadingROI = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getROIReguas(inicio, fim)
      .subscribe({
        next: (data) => {
          this.roiReguas = data;
          this.loadingROI = false;
        },
        error: (error) => {
          console.error('Erro ao carregar ROI de réguas:', error);
          this.loadingROI = false;
        }
      });
  }

  carregarEvolucaoMensal(): void {
    this.loadingEvolucao = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getEvolucaoMensal(inicio, fim)
      .subscribe({
        next: (data) => {
          this.evolucaoMensal = data;
          this.loadingEvolucao = false;
        },
        error: (error) => {
          console.error('Erro ao carregar evolução mensal:', error);
          this.loadingEvolucao = false;
        }
      });
  }

  carregarMelhorHorarioEnvio(): void {
    this.loadingHorarios = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getMelhorHorarioEnvio(inicio, fim)
      .subscribe({
        next: (data) => {
          this.melhorHorario = data;
          this.loadingHorarios = false;
        },
        error: (error) => {
          console.error('Erro ao carregar melhor horário:', error);
          this.loadingHorarios = false;
        }
      });
  }

  carregarReducaoInadimplencia(): void {
    this.loadingReducao = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getReducaoInadimplencia(inicio, fim)
      .subscribe({
        next: (data) => {
          this.reducaoInadimplencia = data;
          this.loadingReducao = false;
        },
        error: (error) => {
          console.error('Erro ao carregar redução de inadimplência:', error);
          this.loadingReducao = false;
        }
      });
  }

  carregarTempoEnvioPagamento(): void {
    this.loadingTempo = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getTempoEnvioPagamento(inicio, fim)
      .subscribe({
        next: (data) => {
          this.tempoEnvioPagamento = data;
          this.loadingTempo = false;
        },
        error: (error) => {
          console.error('Erro ao carregar tempo envio → pagamento:', error);
          this.loadingTempo = false;
        }
      });
  }

  carregarComparativoOmnichannel(): void {
    this.loadingOmnichannel = true;
    const inicio = new Date(this.dataInicio);
    const fim = new Date(this.dataFim);

    this.relatoriosService.getComparativoOmnichannel(inicio, fim)
      .subscribe({
        next: (data) => {
          this.comparativoOmnichannel = data;
          this.loadingOmnichannel = false;
        },
        error: (error) => {
          console.error('Erro ao carregar comparativo omnichannel:', error);
          this.loadingOmnichannel = false;
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

  getCanaisLabel(canais: CanalNotificacao[]): string {
    return canais.map(c => this.getCanalLabel(c)).join(' + ');
  }

  getImpactoClass(interpretacao: string): string {
    switch (interpretacao) {
      case 'Alto':
        return 'impacto-alto';
      case 'Médio':
        return 'impacto-medio';
      case 'Baixo':
        return 'impacto-baixo';
      default:
        return '';
    }
  }

  limparFiltros(): void {
    const hoje = new Date();
    const noventaDiasAtras = new Date();
    noventaDiasAtras.setDate(hoje.getDate() - 90);

    this.dataFim = this.formatDateForInput(hoje);
    this.dataInicio = this.formatDateForInput(noventaDiasAtras);

    this.carregarTodosRelatorios();
  }
}
