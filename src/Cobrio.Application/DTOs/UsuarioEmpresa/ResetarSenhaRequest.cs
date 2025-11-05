using System.ComponentModel.DataAnnotations;

namespace Cobrio.Application.DTOs.UsuarioEmpresa;

public class ResetarSenhaRequest
{
    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    public string NovaSenha { get; set; } = string.Empty;
}
