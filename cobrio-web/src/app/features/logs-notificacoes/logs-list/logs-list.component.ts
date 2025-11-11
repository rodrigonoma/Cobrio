import { Component, OnInit } from '@angular/core';
import { NotificacoesService } from '../../../core/services/notificacoes.service';
import {
  HistoricoNotificacaoResponse,
  StatusNotificacao,
  StatusNotificacaoColors,
  getStatusIcon
} from '../../../core/models/historico-notificacao.model';
import { CanalNotificacao } from '../../../core/models/regra-cobranca.models';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-logs-list',
  templateUrl: './logs-list.component.html',
  styleUrls: ['./logs-list.component.scss']
})
export class LogsListComponent implements OnInit {
  logs: HistoricoNotificacaoResponse[] = [];
  loading = false;
  selectedLog?: HistoricoNotificacaoResponse;
  displayDetalheModal = false;

  // Filtros
  dataInicio?: Date;
  dataFim?: Date;
  statusSelecionado?: StatusNotificacao;
  emailFiltro = '';
  cobrancaIdFiltro = '';
  regraFiltro = '';

  // Opções de status para dropdown
  statusOptions = [
    { label: 'Todos', value: undefined },
    { label: 'Pendente', value: StatusNotificacao.Pendente },
    { label: 'Enviado', value: StatusNotificacao.Enviado },
    { label: 'Entregue', value: StatusNotificacao.Entregue },
    { label: 'Aberto', value: StatusNotificacao.Aberto },
    { label: 'Clicado', value: StatusNotificacao.Clicado },
    { label: 'Erro Temporário', value: StatusNotificacao.SoftBounce },
    { label: 'Erro Permanente', value: StatusNotificacao.HardBounce },
    { label: 'Email Inválido', value: StatusNotificacao.EmailInvalido },
    { label: 'Bloqueado', value: StatusNotificacao.Bloqueado },
    { label: 'Marcado como Spam', value: StatusNotificacao.Reclamacao },
    { label: 'Descadastrado', value: StatusNotificacao.Descadastrado },
    { label: 'Erro no Envio', value: StatusNotificacao.ErroEnvio }
  ];

  constructor(
    private notificacoesService: NotificacoesService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.carregarLogs();
  }

  carregarLogs(): void {
    this.loading = true;

    const filtros = {
      dataInicio: this.dataInicio,
      dataFim: this.dataFim,
      status: this.statusSelecionado,
      emailDestinatario: this.emailFiltro || undefined
    };

    this.notificacoesService.listar(filtros).subscribe({
      next: (logs) => {
        this.logs = logs.map(log => ({
          ...log,
          dataEnvio: new Date(log.dataEnvio),
          dataPrimeiraAbertura: log.dataPrimeiraAbertura ? new Date(log.dataPrimeiraAbertura) : undefined,
          dataUltimaAbertura: log.dataUltimaAbertura ? new Date(log.dataUltimaAbertura) : undefined,
          dataPrimeiroClique: log.dataPrimeiroClique ? new Date(log.dataPrimeiroClique) : undefined,
          dataUltimoClique: log.dataUltimoClique ? new Date(log.dataUltimoClique) : undefined
        }));
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar logs', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Falha ao carregar logs de notificações'
        });
        this.loading = false;
      }
    });
  }

  limparFiltros(): void {
    this.dataInicio = undefined;
    this.dataFim = undefined;
    this.statusSelecionado = undefined;
    this.emailFiltro = '';
    this.cobrancaIdFiltro = '';
    this.regraFiltro = '';
    this.carregarLogs();
  }

  verDetalhes(log: HistoricoNotificacaoResponse): void {
    this.selectedLog = log;
    this.displayDetalheModal = true;
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

  getShortId(id: string): string {
    return id ? id.substring(0, 8) : '';
  }
}
