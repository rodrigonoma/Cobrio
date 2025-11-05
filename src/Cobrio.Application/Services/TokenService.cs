using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Cobrio.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey não configurada");
        _issuer = configuration["JwtSettings:Issuer"] ?? "CobrioAPI";
        _audience = configuration["JwtSettings:Audience"] ?? "CobrioClients";
        _accessTokenExpirationMinutes = int.Parse(configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60");
    }

    public string GenerateAccessToken(UsuarioEmpresa usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email.Endereco),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("TenantId", usuario.EmpresaClienteId.ToString()),
            new Claim("EmpresaClienteId", usuario.EmpresaClienteId.ToString()),
            new Claim("EhProprietario", usuario.EhProprietario.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public Guid? GetTenantIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        var tenantIdClaim = principal?.FindFirst("TenantId")?.Value;

        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
    }
}
