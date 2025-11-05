using System.ComponentModel.DataAnnotations;
using Cobrio.Domain.Enums;

namespace Cobrio.Application.DTOs.UsuarioEmpresa;

public class CreateUsuarioEmpresaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 200 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Perfil é obrigatório")]
    public PerfilUsuario Perfil { get; set; }

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    public string Senha { get; set; } = string.Empty;
}
