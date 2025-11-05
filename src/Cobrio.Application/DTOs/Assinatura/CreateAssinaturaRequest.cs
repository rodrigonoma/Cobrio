namespace Cobrio.Application.DTOs.Assinatura;

public class CreateAssinaturaRequest
{
    public Guid PlanoOfertaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? CpfCnpj { get; set; }

    // Endereço
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? CEP { get; set; }
    public string? Pais { get; set; }

    // Dados de pagamento
    public string? NumeroCartao { get; set; }
    public string? NomeTitular { get; set; }
    public string? ValidadeCartao { get; set; }
    public string? CVV { get; set; }

    // Flags
    public bool IniciarEmTrial { get; set; } = true;
    public DateTime? DataInicio { get; set; }
}
