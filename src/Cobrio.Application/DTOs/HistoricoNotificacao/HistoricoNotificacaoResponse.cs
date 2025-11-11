using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.HistoricoNotificacao;

public class HistoricoNotificacaoResponse
{
    public Guid Id { get; set; }
    public Guid CobrancaId { get; set; }
    public Guid RegraCobrancaId { get; set; }
    public string NomeRegra { get; set; } = string.Empty;

    public CanalNotificacao CanalUtilizado { get; set; }
    public StatusNotificacao Status { get; set; }
    public string StatusTexto { get; set; } = string.Empty;

    public string? EmailDestinatario { get; set; }
    public string? TelefoneDestinatario { get; set; }
    public string? Assunto { get; set; }

    public DateTime DataEnvio { get; set; }
    public string? MensagemErro { get; set; }
    public string? MotivoRejeicao { get; set; }
    public string? CodigoErroProvedor { get; set; }

    // Rastreamento de aberturas
    public int QuantidadeAberturas { get; set; }
    public DateTime? DataPrimeiraAbertura { get; set; }
    public DateTime? DataUltimaAbertura { get; set; }
    public string? IpAbertura { get; set; }
    public string? UserAgentAbertura { get; set; }

    // Rastreamento de cliques
    public int QuantidadeCliques { get; set; }
    public DateTime? DataPrimeiroClique { get; set; }
    public DateTime? DataUltimoClique { get; set; }
    public string? LinkClicado { get; set; }

    public string? MessageIdProvedor { get; set; }

    // Auditoria
    public Guid? UsuarioCriacaoId { get; set; }
    public string? NomeUsuarioCriacao { get; set; }
}
