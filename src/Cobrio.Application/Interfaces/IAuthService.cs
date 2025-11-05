using Cobrio.Application.DTOs.Auth;

namespace Cobrio.Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task RevokeTokenAsync(string refreshToken, string ipAddress);
    Task<bool> ValidateCredentialsAsync(string email, string password);
}
