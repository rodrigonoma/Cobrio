namespace Cobrio.Application.DTOs.Relatorios;

public class MetricasGeraisResponse
{
    public decimal TotalCobrado { get; set; }
    public int TotalEnviadas { get; set; }
    public decimal TaxaAbertura { get; set; }
    public int EmailsAbertos { get; set; }
    public int EmailsEnviados { get; set; }
    public int AssinaturasAtivas { get; set; }
    public decimal VariacaoTotalCobrado { get; set; }
    public decimal VariacaoEnviadas { get; set; }
    public decimal VariacaoAssinaturas { get; set; }
}

public class EnvioPorRegraResponse
{
    public string RegraId { get; set; } = string.Empty;
    public string NomeRegra { get; set; } = string.Empty;
    public int TotalEnvios { get; set; }
    public decimal ValorTotal { get; set; }
}

public class StatusCobrancaResponse
{
    public string Status { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Percentual { get; set; }
}

public class EvolucaoCobrancaResponse
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
}

public class StatusAssinaturasResponse
{
    public int Ativas { get; set; }
    public int Suspensas { get; set; }
    public int Canceladas { get; set; }
}

public class HistoricoImportacaoResponse
{
    public DateTime DataImportacao { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string NomeRegra { get; set; } = string.Empty;
    public int TotalLinhas { get; set; }
    public int LinhasProcessadas { get; set; }
    public int LinhasComErro { get; set; }
}

public class EstatisticasEmailResponse
{
    public int TotalEnviados { get; set; }
    public int TotalAbertos { get; set; }
    public int TotalClicados { get; set; }
    public int TotalBounces { get; set; }
    public decimal TaxaAbertura { get; set; }
    public decimal TaxaClique { get; set; }
}
