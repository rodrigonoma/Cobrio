export interface PlanoOferta {
  id: string;
  empresaClienteId: string;
  nome: string;
  descricao?: string;
  valor: number;
  moeda: string;
  tipoCiclo: TipoCiclo;
  periodoTrial: number;
  limiteUsuarios?: number;
  ativo: boolean;
  permiteUpgrade: boolean;
  permiteDowngrade: boolean;
  criadoEm: string;
  atualizadoEm: string;
}

export interface CreatePlanoOfertaRequest {
  nome: string;
  descricao?: string;
  valor: number;
  tipoCiclo: TipoCiclo;
  periodoTrial: number;
  limiteUsuarios?: number;
  permiteUpgrade: boolean;
  permiteDowngrade: boolean;
}

export interface UpdatePlanoOfertaRequest {
  nome?: string;
  descricao?: string;
  valor?: number;
  periodoTrial?: number;
  limiteUsuarios?: number;
  permiteUpgrade?: boolean;
  permiteDowngrade?: boolean;
}

export type TipoCiclo = 'Mensal' | 'Trimestral' | 'Semestral' | 'Anual';

export const TIPOS_CICLO: { label: string; value: TipoCiclo }[] = [
  { label: 'Mensal', value: 'Mensal' },
  { label: 'Trimestral', value: 'Trimestral' },
  { label: 'Semestral', value: 'Semestral' },
  { label: 'Anual', value: 'Anual' }
];
