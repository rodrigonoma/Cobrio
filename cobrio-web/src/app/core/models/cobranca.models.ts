export interface CreateCobrancaRequest {
  /**
   * Email do destinatário (obrigatório para canal Email)
   */
  Email?: string;

  /**
   * Telefone do destinatário com DDD (obrigatório para canais SMS/WhatsApp)
   * Formato: +5511999999999
   */
  Telefone?: string;

  /**
   * Nome do cliente (obrigatório se configurado na regra)
   */
  NomeCliente?: string;

  /**
   * Variáveis customizadas para substituição no template
   * Ex: { "Valor": "150.00", "LinkPagamento": "https://..." }
   */
  Payload: { [key: string]: any };

  /**
   * Data de vencimento da cobrança (sempre obrigatório)
   * Formatos aceitos:
   * - yyyy-MM-dd (ex: 2025-12-31)
   * - yyyy-MM-dd HH:mm:ss (ex: 2025-12-31 23:59:59)
   * - dd/MM/yyyy (ex: 31/12/2025)
   * - dd/MM/yyyy HH:mm (ex: 31/12/2025 23:59)
   */
  DataVencimento?: string;
}
