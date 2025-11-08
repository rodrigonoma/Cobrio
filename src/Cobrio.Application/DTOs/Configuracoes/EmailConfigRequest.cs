namespace Cobrio.Application.DTOs.Configuracoes;

public class EmailConfigRequest
{
    public string? EmailRemetente { get; set; }
    public string? NomeRemetente { get; set; }
    public string? EmailReplyTo { get; set; }
}
