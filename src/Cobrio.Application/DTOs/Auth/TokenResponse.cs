namespace Cobrio.Application.DTOs.Auth;

public class TokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string TokenType { get; set; } = "Bearer";

    public UserInfo User { get; set; } = null!;
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Perfil { get; set; } = null!;
    public bool EhProprietario { get; set; }
    public bool Ativo { get; set; }
    public Guid EmpresaClienteId { get; set; }
    public string EmpresaClienteNome { get; set; } = null!;
}
