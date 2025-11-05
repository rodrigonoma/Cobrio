using System.ComponentModel.DataAnnotations;
using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.UsuarioEmpresa;

public class UpdateUsuarioEmpresaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 200 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Perfil é obrigatório")]
    public PerfilUsuario Perfil { get; set; }

    [Required(ErrorMessage = "Status Ativo é obrigatório")]
    public bool Ativo { get; set; }
}
