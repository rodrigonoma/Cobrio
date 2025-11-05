using System.Security.Claims;

namespace Cobrio.API.Middleware;

/// <summary>
/// Middleware que extrai o TenantId do JWT e o armazena no HttpContext
/// para ser usado pelos Query Filters do EF Core
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extrair TenantId do token JWT (se autenticado)
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value
                ?? context.User.FindFirst("EmpresaClienteId")?.Value;

            if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
                _logger.LogDebug("TenantId {TenantId} extraído do token", tenantId);
            }
            else
            {
                _logger.LogWarning("Token JWT não contém TenantId válido");
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method para registrar o middleware
/// </summary>
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
