import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import {
  HistoricoNotificacaoResponse,
  StatusNotificacao,
  StatusNotificacaoColors,
  getStatusIcon
} from '../../../core/models/historico-notificacao.model';
import { CanalNotificacao } from '../../../core/models/regra-cobranca.models';

interface TimelineEvent {
  id: string;
  statusAnterior: StatusNotificacao;
  statusNovo: StatusNotificacao;
  statusNovoTexto: string;
  dataMudanca: Date;
  detalhes?: string;
  ipOrigem?: string;
  userAgent?: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-log-detail-modal',
  templateUrl: './log-detail-modal.component.html',
  styleUrls: ['./log-detail-modal.component.scss']
})
export class LogDetailModalComponent implements OnInit {
  @Input() display = false;
  @Input() log!: HistoricoNotificacaoResponse;

  @Output() displayChange = new EventEmitter<boolean>();

  timeline: TimelineEvent[] = [];
  selectedEvent?: TimelineEvent;

  ngOnInit(): void {
    if (this.log) {
      this.construirTimeline();
    }
  }

  get displayValue(): boolean {
    return this.display;
  }

  set displayValue(value: boolean) {
    this.display = value;
    this.displayChange.emit(value);
    if (value && this.log) {
      this.construirTimeline();
    }
  }

  construirTimeline(): void {
    const eventos: TimelineEvent[] = [];

    // Evento 1: Envio inicial
    eventos.push({
      id: '1',
      statusAnterior: StatusNotificacao.Pendente,
      statusNovo: StatusNotificacao.Enviado,
      statusNovoTexto: 'Enviado',
      dataMudanca: this.log.dataEnvio,
      detalhes: 'Email enviado com sucesso',
      icon: 'pi pi-send',
      color: '#3b82f6'
    });

    // Evento 2: Entrega (se status >= Entregue)
    if (this.log.status >= StatusNotificacao.Entregue) {
      const dataEntrega = new Date(this.log.dataEnvio);
      dataEntrega.setSeconds(dataEntrega.getSeconds() + 5);

      eventos.push({
        id: '2',
        statusAnterior: StatusNotificacao.Enviado,
        statusNovo: StatusNotificacao.Entregue,
        statusNovoTexto: 'Entregue',
        dataMudanca: dataEntrega,
        detalhes: 'Email entregue na caixa de entrada',
        icon: 'pi pi-check-circle',
        color: '#10b981'
      });
    }

    // Evento 3: Primeira abertura
    if (this.log.dataPrimeiraAbertura) {
      const tempoAteAbertura = this.calcularDiferenca(this.log.dataEnvio, this.log.dataPrimeiraAbertura);

      eventos.push({
        id: '3',
        statusAnterior: StatusNotificacao.Entregue,
        statusNovo: StatusNotificacao.Aberto,
        statusNovoTexto: 'Aberto pela 1ª vez',
        dataMudanca: this.log.dataPrimeiraAbertura,
        detalhes: `Aberto ${tempoAteAbertura} após o envio`,
        ipOrigem: this.log.ipAbertura,
        userAgent: this.log.userAgentAbertura,
        icon: 'pi pi-eye',
        color: '#8b5cf6'
      });
    }

    // Evento 4: Primeiro clique
    if (this.log.dataPrimeiroClique) {
      const tempoAteClique = this.calcularDiferenca(this.log.dataEnvio, this.log.dataPrimeiroClique);

      eventos.push({
        id: '4',
        statusAnterior: StatusNotificacao.Aberto,
        statusNovo: StatusNotificacao.Clicado,
        statusNovoTexto: 'Link Clicado',
        dataMudanca: this.log.dataPrimeiroClique,
        detalhes: `Clicado ${tempoAteClique} após o envio`,
        icon: 'pi pi-link',
        color: '#f59e0b'
      });
    }

    this.timeline = eventos;
  }

  getTempoRelativo(data: Date): string {
    if (!data) return '-';

    const agora = new Date();
    const dataEvento = new Date(data);

    // Validar se a data é válida
    if (isNaN(dataEvento.getTime())) {
      return '-';
    }

    const diff = agora.getTime() - dataEvento.getTime();
    const segundos = Math.floor(diff / 1000);
    const minutos = Math.floor(segundos / 60);
    const horas = Math.floor(minutos / 60);
    const dias = Math.floor(horas / 24);

    if (dias > 0) return `há ${dias} dia${dias > 1 ? 's' : ''}`;
    if (horas > 0) return `há ${horas} hora${horas > 1 ? 's' : ''}`;
    if (minutos > 0) return `há ${minutos} minuto${minutos > 1 ? 's' : ''}`;
    if (segundos > 10) return `há ${segundos} segundos`;
    return 'agora mesmo';
  }

  calcularDiferenca(dataInicio: Date, dataFim: Date): string {
    const diff = new Date(dataFim).getTime() - new Date(dataInicio).getTime();
    const segundos = Math.floor(diff / 1000);
    const minutos = Math.floor(segundos / 60);
    const horas = Math.floor(minutos / 60);

    if (horas > 0) return `${horas}h ${minutos % 60}min`;
    if (minutos > 0) return `${minutos}min ${segundos % 60}s`;
    return `${segundos}s`;
  }

  getTaxaAbertura(): number {
    return this.log.quantidadeAberturas > 0 ? 100 : 0;
  }

  getTaxaClique(): number {
    if (this.log.quantidadeAberturas === 0) return 0;
    return Math.round((this.log.quantidadeCliques / this.log.quantidadeAberturas) * 100);
  }

  getEngajamentoScore(): number {
    let score = 0;
    if (this.log.quantidadeAberturas > 0) score += 50;
    if (this.log.quantidadeCliques > 0) score += 50;
    return score;
  }

  getEngajamentoLabel(): string {
    const score = this.getEngajamentoScore();
    if (score === 100) return 'Excelente';
    if (score >= 50) return 'Bom';
    if (score > 0) return 'Baixo';
    return 'Sem engajamento';
  }

  getStatusSeverity(status: StatusNotificacao): string {
    return StatusNotificacaoColors[status] || 'info';
  }

  getStatusIcon(status: StatusNotificacao): string {
    return getStatusIcon(status);
  }

  getCanalIcon(canal: CanalNotificacao): string {
    switch (canal) {
      case CanalNotificacao.Email:
        return 'pi pi-envelope';
      case CanalNotificacao.SMS:
        return 'pi pi-mobile';
      case CanalNotificacao.WhatsApp:
        return 'pi pi-whatsapp';
      default:
        return 'pi pi-send';
    }
  }

  formatDate(date?: Date): string {
    if (!date) return '-';
    return new Date(date).toLocaleString('pt-BR');
  }

  hasEngagement(): boolean {
    return this.log.quantidadeAberturas > 0 || this.log.quantidadeCliques > 0;
  }

  hasError(): boolean {
    return !!this.log.mensagemErro || !!this.log.motivoRejeicao;
  }

  selectEvent(event: TimelineEvent): void {
    this.selectedEvent = this.selectedEvent?.id === event.id ? undefined : event;
  }
}
