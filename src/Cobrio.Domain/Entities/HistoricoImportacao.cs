using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

public class HistoricoImportacao : BaseEntity
{
    public Guid RegraCobrancaId { get; private set; }
    public Guid EmpresaClienteId { get; private set; }
    public Guid? UsuarioId { get; private set; }

    // Informações do arquivo/origem
    public string NomeArquivo { get; private set; }
    public DateTime DataImportacao { get; private set; }
    public OrigemImportacao Origem { get; private set; }

    // Resultados da importação
    public int TotalLinhas { get; private set; }
    public int LinhasProcessadas { get; private set; }
    public int LinhasComErro { get; private set; }
    public StatusImportacao Status { get; private set; }

    // Detalhes dos erros (JSON)
    public string? ErrosJson { get; private set; }

    // Navegação
    public RegraCobranca RegraCobranca { get; private set; } = null!;
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;
    public UsuarioEmpresa? Usuario { get; private set; }

    // Construtor para EF Core
    private HistoricoImportacao() { }

    public HistoricoImportacao(
        Guid regraCobrancaId,
        Guid empresaClienteId,
        Guid? usuarioId,
        string nomeArquivo,
        int totalLinhas,
        int linhasProcessadas,
        int linhasComErro,
        StatusImportacao status,
        OrigemImportacao origem,
        string? errosJson = null)
    {
        if (regraCobrancaId == Guid.Empty)
            throw new ArgumentException("RegraCobrancaId inválido", nameof(regraCobrancaId));

        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new ArgumentException("Nome do arquivo não pode ser vazio", nameof(nomeArquivo));

        RegraCobrancaId = regraCobrancaId;
        EmpresaClienteId = empresaClienteId;
        UsuarioId = usuarioId;
        NomeArquivo = nomeArquivo;
        DataImportacao = DateTime.Now;
        TotalLinhas = totalLinhas;
        LinhasProcessadas = linhasProcessadas;
        LinhasComErro = linhasComErro;
        Status = status;
        Origem = origem;
        ErrosJson = errosJson;
    }
}
