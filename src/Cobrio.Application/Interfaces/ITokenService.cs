using Cobrio.Domain.Entities;
using System.Security.Claims;

namespace Cobrio.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UsuarioEmpresa usuario);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    Guid? GetTenantIdFromToken(string token);
}
