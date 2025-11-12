using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.Relatorios;

/// <summary>
/// Dashboard completo de consumo de canais
/// </summary>
public class DashboardConsumoResponse
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    // Totalizadores
    public ConsumoTotaisResponse Totais { get; set; } = new();

    // Consumo por canal
    public List<ConsumoPorCanalResponse> ConsumoPorCanal { get; set; } = new();

    // Consumo por usuário
    public List<ConsumoPorUsuarioResponse> ConsumoPorUsuario { get; set; } = new();

    // Consumo por regra
    public List<ConsumoPorReguaResponse> ConsumoPorRegua { get; set; } = new();

    // Evolução temporal (dia a dia)
    public List<ConsumoTemporalResponse> EvolucaoTemporal { get; set; } = new();
}

/// <summary>
/// Totalizadores gerais de consumo
/// </summary>
public class ConsumoTotaisResponse
{
    public int TotalEnvios { get; set; }
    public int TotalEmails { get; set; }
    public int TotalSMS { get; set; }
    public int TotalWhatsApp { get; set; }

    // Média diária
    public decimal MediaEnviosPorDia { get; set; }
}

/// <summary>
/// Consumo detalhado por canal
/// </summary>
public class ConsumoPorCanalResponse
{
    public CanalNotificacao Canal { get; set; }
    public string NomeCanal { get; set; } = string.Empty;
    public int TotalEnvios { get; set; }
    public int Sucessos { get; set; }
    public int Falhas { get; set; }
    public decimal TaxaSucesso { get; set; }
    public decimal PercentualDoTotal { get; set; }
}

/// <summary>
/// Consumo por usuário
/// </summary>
public class ConsumoPorUsuarioResponse
{
    public Guid? UsuarioId { get; set; }
    public string NomeUsuario { get; set; } = "Sistema";
    public int TotalEnvios { get; set; }
    public int EnviosEmail { get; set; }
    public int EnviosSMS { get; set; }
    public int EnviosWhatsApp { get; set; }
    public decimal PercentualDoTotal { get; set; }
}

/// <summary>
/// Consumo por régua de cobrança
/// </summary>
public class ConsumoPorReguaResponse
{
    public Guid ReguaId { get; set; }
    public string NomeRegua { get; set; } = string.Empty;
    public CanalNotificacao Canal { get; set; }
    public int TotalEnvios { get; set; }
    public decimal PercentualDoTotal { get; set; }
}

/// <summary>
/// Evolução temporal do consumo
/// </summary>
public class ConsumoTemporalResponse
{
    public DateTime Data { get; set; }
    public int TotalEnvios { get; set; }
    public int EnviosEmail { get; set; }
    public int EnviosSMS { get; set; }
    public int EnviosWhatsApp { get; set; }
}
