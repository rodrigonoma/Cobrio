import { PerfilUsuario } from './usuario-empresa.models';

export enum TipoAcao {
  Menu = 'Menu',
  CRUD = 'CRUD',
  Especial = 'Especial'
}

export interface Modulo {
  id: string;
  nome: string;
  chave: string;
  descricao: string;
  icone: string;
  rota: string;
  ordem: number;
  ativo: boolean;
}

export interface Acao {
  id: string;
  nome: string;
  chave: string;
  descricao: string;
  tipoAcao: TipoAcao;
  ativa: boolean;
}

export interface AcaoPermissao {
  acaoId: string;
  acaoNome: string;
  acaoChave: string;
  tipoAcao: TipoAcao;
  permitido: boolean;
}

export interface ModuloPermissao {
  moduloId: string;
  moduloNome: string;
  moduloChave: string;
  moduloIcone: string;
  moduloRota: string;
  acoes: AcaoPermissao[];
}

export interface PermissaoPerfil {
  perfilUsuario: PerfilUsuario;
  modulos: ModuloPermissao[];
}

export interface ConfigurarPermissoesRequest {
  perfilUsuario: PerfilUsuario;
  permissoes: { [moduloId: string]: { [acaoId: string]: boolean } };
}
