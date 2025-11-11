import { CanalNotificacao } from './regra-cobranca.models';

/**
 * Status de notificação - sincronizado com backend
 */
export enum StatusNotificacao {
  Pendente = 0,
  Enviado = 1,
  Entregue = 2,
  Aberto = 3,
  Clicado = 4,
  SoftBounce = 10,
  Adiado = 11,
  HardBounce = 20,
  EmailInvalido = 21,
  Bloqueado = 22,
  Reclamacao = 30,
  Descadastrado = 31,
  ErroEnvio = 40,

  // Aliases para compatibilidade
  Sucesso = 2,
  Falha = 40
}

/**
 * Response da API de Histórico de Notificações
 */
export interface HistoricoNotificacaoResponse {
  id: string;
  cobrancaId: string;
  regraCobrancaId: string;
  nomeRegra: string;
  canalUtilizado: CanalNotificacao;
  status: StatusNotificacao;
  statusTexto: string;
  emailDestinatario?: string;
  telefoneDestinatario?: string;
  assunto?: string;
  dataEnvio: Date;
  mensagemErro?: string;
  motivoRejeicao?: string;
  codigoErroProvedor?: string;
  quantidadeAberturas: number;
  dataPrimeiraAbertura?: Date;
  dataUltimaAbertura?: Date;
  ipAbertura?: string;
  userAgentAbertura?: string;
  quantidadeCliques: number;
  dataPrimeiroClique?: Date;
  dataUltimoClique?: Date;
  linkClicado?: string;
  messageIdProvedor?: string;
  usuarioCriacaoId?: string;
  nomeUsuarioCriacao?: string;
}

/**
 * Mapeamento de cores para status
 */
export const StatusNotificacaoColors: Record<StatusNotificacao, string> = {
  [StatusNotificacao.Pendente]: 'info',
  [StatusNotificacao.Enviado]: 'info',
  [StatusNotificacao.Entregue]: 'success',
  [StatusNotificacao.Aberto]: 'success',
  [StatusNotificacao.Clicado]: 'success',
  [StatusNotificacao.SoftBounce]: 'warning',
  [StatusNotificacao.Adiado]: 'warning',
  [StatusNotificacao.HardBounce]: 'danger',
  [StatusNotificacao.EmailInvalido]: 'danger',
  [StatusNotificacao.Bloqueado]: 'danger',
  [StatusNotificacao.Reclamacao]: 'danger',
  [StatusNotificacao.Descadastrado]: 'warning',
  [StatusNotificacao.ErroEnvio]: 'danger',
  [StatusNotificacao.Sucesso]: 'success',
  [StatusNotificacao.Falha]: 'danger'
};

/**
 * Helper para obter ícone do status
 */
export function getStatusIcon(status: StatusNotificacao): string {
  switch (status) {
    case StatusNotificacao.Pendente:
      return 'pi pi-clock';
    case StatusNotificacao.Enviado:
      return 'pi pi-send';
    case StatusNotificacao.Entregue:
    case StatusNotificacao.Sucesso:
      return 'pi pi-check-circle';
    case StatusNotificacao.Aberto:
      return 'pi pi-envelope-open';
    case StatusNotificacao.Clicado:
      return 'pi pi-external-link';
    case StatusNotificacao.SoftBounce:
    case StatusNotificacao.Adiado:
      return 'pi pi-exclamation-triangle';
    case StatusNotificacao.HardBounce:
    case StatusNotificacao.EmailInvalido:
    case StatusNotificacao.Bloqueado:
    case StatusNotificacao.Reclamacao:
    case StatusNotificacao.ErroEnvio:
    case StatusNotificacao.Falha:
      return 'pi pi-times-circle';
    case StatusNotificacao.Descadastrado:
      return 'pi pi-user-minus';
    default:
      return 'pi pi-info-circle';
  }
}
