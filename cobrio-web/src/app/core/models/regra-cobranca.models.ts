export enum CanalNotificacao {
  Email = 1,
  SMS = 2,
  WhatsApp = 3
}

export enum TipoMomento {
  Antes = 1,
  Depois = 2,
  Exatamente = 3
}

export enum UnidadeTempo {
  Minutos = 1,
  Horas = 2,
  Dias = 3
}

export interface RegraCobranca {
  id: string;
  empresaClienteId: string;
  nome: string;
  descricao?: string;
  ativa: boolean;
  ehPadrao: boolean;
  tipoMomento: TipoMomento;
  valorTempo: number;
  unidadeTempo: UnidadeTempo;
  canalNotificacao: CanalNotificacao;
  templateNotificacao: string;
  variaveisObrigatorias: string[];
  variaveisObrigatoriasSistema?: string;  // JSON string com lista de variáveis
  tokenWebhook: string;
  criadoEm: Date;
  atualizadoEm: Date;
}

export interface CreateRegraCobrancaRequest {
  nome: string;
  descricao?: string;
  tipoMomento: TipoMomento;
  valorTempo: number;
  unidadeTempo: UnidadeTempo;
  canalNotificacao: CanalNotificacao;
  templateNotificacao: string;
  variaveisObrigatoriasSistema?: string[];
}

export interface UpdateRegraCobrancaRequest {
  nome?: string;
  descricao?: string;
  tipoMomento?: TipoMomento;
  valorTempo?: number;
  unidadeTempo?: UnidadeTempo;
  canalNotificacao?: CanalNotificacao;
  templateNotificacao?: string;
  variaveisObrigatoriasSistema?: string[];
  ativa?: boolean;
}

export interface ErroValidacaoLinha {
  numeroLinha: number;
  tipoErro: string;
  descricao: string;
  valorInvalido?: string;
}

export interface ImportacaoResultado {
  sucesso: boolean;
  mensagem: string;
  linhasProcessadas: number;
  linhasComErro: number;
  erros?: ErroValidacaoLinha[];
}
