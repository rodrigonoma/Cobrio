export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresAt: string;
  user: Usuario;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
}

export interface Usuario {
  id: string;
  empresaClienteId: string;
  nome: string;
  email: string;
  perfil: string;
  ehProprietario: boolean;
  ativo: boolean;
  empresaClienteNome: string;
}

export interface EmpresaCliente {
  id: string;
  nome: string;
  cnpj: string;
  email: string;
  telefone?: string;
  planoCobrioId: number;
  ativo: boolean;
}

export interface ValidateTokenResponse {
  valid: boolean;
  expiresAt?: string;
}
