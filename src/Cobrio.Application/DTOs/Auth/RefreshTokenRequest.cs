using System.ComponentModel.DataAnnotations;

namespace Cobrio.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "O refresh token é obrigatório")]
    public string RefreshToken { get; set; } = null!;
}
