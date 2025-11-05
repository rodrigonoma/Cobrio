export enum PerfilUsuario {
  Admin = 1,
  Operador = 2
}

export interface UsuarioEmpresa {
  id: string;
  empresaClienteId: string;
  nome: string;
  email: string;
  perfil: PerfilUsuario;
  perfilDescricao: string;
  ativo: boolean;
  ehProprietario: boolean;
  ultimoAcesso?: Date;
  criadoEm: Date;
  atualizadoEm: Date;
}

export interface CreateUsuarioEmpresaRequest {
  nome: string;
  email: string;
  perfil: PerfilUsuario;
  senha: string;
}

export interface UpdateUsuarioEmpresaRequest {
  nome: string;
  perfil: PerfilUsuario;
  ativo: boolean;
}

export interface ResetarSenhaRequest {
  novaSenha: string;
}
