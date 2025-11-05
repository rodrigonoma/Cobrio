using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.UsuarioEmpresa;

public class UsuarioEmpresaResponse
{
    public Guid Id { get; set; }
    public Guid EmpresaClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public string PerfilDescricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool EhProprietario { get; set; }
    public DateTime? UltimoAcesso { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
