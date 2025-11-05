using System.ComponentModel.DataAnnotations;

namespace Cobrio.Application.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Password { get; set; } = null!;
}
