export interface Assinatura {
  id: string;
  empresaClienteId: string;
  assinanteId: string;
  planoOfertaId: string;
  planoNome: string;
  assinanteNome: string;
  assinanteEmail: string;
  status: StatusAssinatura;
  valor: number; // em centavos
  tipoCiclo: string;
  dataInicio: string;
  dataProximaCobranca?: string;
  dataCancelamento?: string;
  dataSuspensao?: string;
  dataExpiracao?: string;
  trialInicio?: string;
  trialFim?: string;
  emTrial: boolean;
  criadoEm: string;
  atualizadoEm: string;
}

export type StatusAssinatura =
  | 'Ativa'
  | 'EmTrial'
  | 'Suspensa'
  | 'Cancelada'
  | 'Expirada'
  | 'PendenteAtivacao';

export interface CreateAssinaturaRequest {
  planoOfertaId: string;
  nome: string;
  email: string;
  telefone?: string;
  cpfCnpj?: string;

  // Endereço
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  cep?: string;
  pais?: string;

  // Dados de Pagamento
  numeroCartao?: string;
  nomeTitular?: string;
  validadeCartao?: string;
  cvv?: string;

  iniciarEmTrial?: boolean;
  dataInicio?: string;
}

export interface UpdateAssinaturaRequest {
  novoPlanoId?: string;
  telefone?: string;

  // Endereço
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  cep?: string;
  pais?: string;
}

export interface CancelarAssinaturaRequest {
  motivo?: string;
  cancelarImediatamente?: boolean;
}

export const STATUS_ASSINATURA_OPTIONS = [
  { label: 'Ativa', value: 'Ativa', severity: 'success' },
  { label: 'Em Trial', value: 'EmTrial', severity: 'info' },
  { label: 'Suspensa', value: 'Suspensa', severity: 'warning' },
  { label: 'Cancelada', value: 'Cancelada', severity: 'danger' },
  { label: 'Expirada', value: 'Expirada', severity: 'danger' },
  { label: 'Pendente Ativação', value: 'PendenteAtivacao', severity: 'warning' }
];
