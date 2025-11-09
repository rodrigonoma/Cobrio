export interface TemplateEmail {
  id: string;
  nome: string;
  descricao?: string;
  conteudoHtml: string;
  subjectEmail?: string;
  variaveisObrigatorias: string[];
  variaveisObrigatoriasSistema: string[];
  canalSugerido?: number;
  criadoEm: Date;
  atualizadoEm: Date;
}

export interface CreateTemplateEmailRequest {
  nome: string;
  descricao?: string;
  conteudoHtml: string;
  subjectEmail?: string;
  variaveisObrigatoriasSistema?: string[];
  canalSugerido?: number;
}

export interface UpdateTemplateEmailRequest {
  nome?: string;
  descricao?: string;
  conteudoHtml?: string;
  subjectEmail?: string;
  variaveisObrigatoriasSistema?: string[];
  canalSugerido?: number;
}
