using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

public class MetodoPagamento : BaseEntity
{
    public Guid AssinanteId { get; private set; }
    public Guid EmpresaClienteId { get; private set; }

    public TipoMetodoPagamento Tipo { get; private set; }

    // Tokenizado (NUNCA armazenar dados completos de cartão)
    public string? TokenGateway { get; private set; }
    public string? GatewayProvider { get; private set; }

    // Dados seguros para exibição
    public string? UltimosDigitos { get; private set; }
    public string? Bandeira { get; private set; }
    public string? NomeTitular { get; private set; }

    public DateTime? DataValidade { get; private set; }
    public bool Principal { get; private set; }
    public bool Ativo { get; private set; }

    // Navegação
    public Assinante Assinante { get; private set; } = null!;
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    // Construtor para EF Core
    private MetodoPagamento() { }

    public MetodoPagamento(
        Guid assinanteId,
        Guid empresaClienteId,
        TipoMetodoPagamento tipo,
        string? tokenGateway = null,
        string? gatewayProvider = null,
        string? ultimosDigitos = null,
        string? bandeira = null,
        string? nomeTitular = null,
        DateTime? dataValidade = null,
        bool principal = false)
    {
        if (assinanteId == Guid.Empty)
            throw new ArgumentException("AssinanteId inválido", nameof(assinanteId));

        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        AssinanteId = assinanteId;
        EmpresaClienteId = empresaClienteId;
        Tipo = tipo;
        TokenGateway = tokenGateway;
        GatewayProvider = gatewayProvider;
        UltimosDigitos = ultimosDigitos;
        Bandeira = bandeira;
        NomeTitular = nomeTitular;
        DataValidade = dataValidade;
        Principal = principal;
        Ativo = true;
    }

    public void DefinirComoPrincipal()
    {
        Principal = true;
        AtualizarDataModificacao();
    }

    public void RemoverComoPrincipal()
    {
        Principal = false;
        AtualizarDataModificacao();
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }

    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }

    public bool EstaVencido()
    {
        if (DataValidade == null) return false;
        return DateTime.UtcNow > DataValidade.Value;
    }

    public bool VenceEmDias(int dias)
    {
        if (DataValidade == null) return false;
        return (DataValidade.Value - DateTime.UtcNow).Days <= dias;
    }

    public void AtualizarToken(string novoToken, string? novoBandeira = null, DateTime? novaValidade = null)
    {
        TokenGateway = novoToken;

        if (novoBandeira != null)
            Bandeira = novoBandeira;

        if (novaValidade != null)
            DataValidade = novaValidade;

        AtualizarDataModificacao();
    }
}
