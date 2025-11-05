export enum StatusImportacao {
  Sucesso = 1,
  Parcial = 2,
  Erro = 3
}

export enum OrigemImportacao {
  Excel = 1,
  Webhook = 2,
  Manual = 3,
  Json = 4
}

export interface ErroImportacao {
  numeroLinha: number;
  tipoErro: string;
  descricao: string;
  valorInvalido?: string;
}

export interface HistoricoImportacao {
  id: string;
  regraCobrancaId: string;
  nomeRegra: string;
  usuarioId?: string;
  nomeUsuario?: string;
  nomeArquivo: string;
  dataImportacao: Date;
  origem: OrigemImportacao;
  origemDescricao: string;
  totalLinhas: number;
  linhasProcessadas: number;
  linhasComErro: number;
  status: StatusImportacao;
  statusDescricao: string;
  erros?: ErroImportacao[];
}
