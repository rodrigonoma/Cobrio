namespace Cobrio.Application.DTOs.PlanoOferta;

public class PlanoOfertaResponse
{
    public Guid Id { get; set; }
    public Guid EmpresaClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
    public string TipoCiclo { get; set; } = string.Empty;
    public int PeriodoTrial { get; set; }
    public int? LimiteUsuarios { get; set; }
    public bool Ativo { get; set; }
    public bool PermiteUpgrade { get; set; }
    public bool PermiteDowngrade { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}

public class CreatePlanoOfertaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public string TipoCiclo { get; set; } = "Mensal";
    public int PeriodoTrial { get; set; } = 0;
    public int? LimiteUsuarios { get; set; }
    public bool PermiteUpgrade { get; set; } = true;
    public bool PermiteDowngrade { get; set; } = true;
}

public class UpdatePlanoOfertaRequest
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public decimal? Valor { get; set; }
    public int? PeriodoTrial { get; set; }
    public int? LimiteUsuarios { get; set; }
    public bool? PermiteUpgrade { get; set; }
    public bool? PermiteDowngrade { get; set; }
}
