using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.HistoricoImportacao;

public class HistoricoImportacaoResponse
{
    public Guid Id { get; set; }
    public Guid RegraCobrancaId { get; set; }
    public string NomeRegra { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
    public string? NomeUsuario { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public DateTime DataImportacao { get; set; }
    public OrigemImportacao Origem { get; set; }
    public string OrigemDescricao { get; set; } = string.Empty;
    public int TotalLinhas { get; set; }
    public int LinhasProcessadas { get; set; }
    public int LinhasComErro { get; set; }
    public StatusImportacao Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public List<ErroImportacaoDto>? Erros { get; set; }
}

public class ErroImportacaoDto
{
    public int NumeroLinha { get; set; }
    public string TipoErro { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ValorInvalido { get; set; }
}
