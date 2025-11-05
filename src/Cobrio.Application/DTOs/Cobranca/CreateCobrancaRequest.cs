using System.ComponentModel.DataAnnotations;

namespace Cobrio.Application.DTOs.Cobranca;

public class CreateCobrancaRequest
{
    /// <summary>
    /// Email do destinatário (obrigatório para canal Email)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Telefone do destinatário com DDD (obrigatório para canais SMS/WhatsApp)
    /// Formato: +5511999999999
    /// </summary>
    public string? Telefone { get; set; }

    /// <summary>
    /// Nome do cliente (obrigatório se configurado na regra)
    /// </summary>
    public string? NomeCliente { get; set; }

    /// <summary>
    /// Variáveis customizadas para substituição no template
    /// Ex: { "Valor": "150.00", "LinkPagamento": "https://..." }
    /// </summary>
    public Dictionary<string, object> Payload { get; set; } = new();

    /// <summary>
    /// Data de vencimento da cobrança (sempre obrigatório)
    /// Formatos aceitos:
    /// - yyyy-MM-dd (ex: 2025-12-31)
    /// - yyyy-MM-dd HH:mm:ss (ex: 2025-12-31 23:59:59)
    /// - dd/MM/yyyy (ex: 31/12/2025)
    /// - dd/MM/yyyy HH:mm (ex: 31/12/2025 23:59)
    /// </summary>
    public string? DataVencimento { get; set; }
}
