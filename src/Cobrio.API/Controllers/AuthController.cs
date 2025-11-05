using Cobrio.Application.DTOs.Auth;
using Cobrio.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza login e retorna access token + refresh token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.LoginAsync(request, ipAddress);

            _logger.LogInformation("Login bem-sucedido para usuário {Email}", request.Email);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Tentativa de login falhou para {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar login para {Email}", request.Email);
            return BadRequest(new { message = "Erro ao processar login" });
        }
    }

    /// <summary>
    /// Renova o access token usando o refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

            _logger.LogInformation("Refresh token bem-sucedido");

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Tentativa de refresh token falhou: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar refresh token");
            return Unauthorized(new { message = "Token inválido" });
        }
    }

    /// <summary>
    /// Revoga um refresh token (logout)
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

            _logger.LogInformation("Token revogado com sucesso");

            return Ok(new { message = "Token revogado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar token");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint para validar se o token atual está válido
    /// </summary>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ValidateToken()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var tenantId = User.FindFirst("TenantId")?.Value;

        return Ok(new
        {
            isValid = true,
            userId,
            email,
            tenantId,
            message = "Token válido"
        });
    }

    private string GetIpAddress()
    {
        // Tenta obter o IP real considerando proxies (X-Forwarded-For)
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
