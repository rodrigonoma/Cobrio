namespace Cobrio.Application.DTOs.Assinatura;

public class UpdateAssinaturaRequest
{
    public Guid? NovoPlanoId { get; set; }
    public string? Telefone { get; set; }

    // Endereço
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? CEP { get; set; }
    public string? Pais { get; set; }
}

public class CancelarAssinaturaRequest
{
    public string? Motivo { get; set; }
    public bool CancelarImediatamente { get; set; } = false;
}
