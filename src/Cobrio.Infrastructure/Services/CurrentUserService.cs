using System.Security.Claims;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Cobrio.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? UserEmail
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        }
    }

    public string? UserName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }

    public PerfilUsuario? Perfil
    {
        get
        {
            var perfilClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<PerfilUsuario>(perfilClaim, out var perfil) ? perfil : null;
        }
    }

    public Guid? EmpresaClienteId
    {
        get
        {
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
        }
    }

    public bool EhProprietario
    {
        get
        {
            // Você pode adicionar uma claim específica para isso no JWT
            // Por exemplo: "EhProprietario": "true"
            var proprietarioClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("EhProprietario")?.Value;
            return bool.TryParse(proprietarioClaim, out var ehProprietario) && ehProprietario;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    public bool PodeVisualizarDadosDe(Guid? usuarioCriacaoId, PerfilUsuario? perfilCriador)
    {
        // Se não estiver autenticado, não pode ver nada
        if (!IsAuthenticated || !UserId.HasValue)
            return false;

        // Proprietário vê tudo
        if (EhProprietario)
            return true;

        // Se não tem dono definido, todos podem ver (dados antigos)
        if (!usuarioCriacaoId.HasValue)
            return true;

        // Operador só vê o que ele criou
        if (Perfil == PerfilUsuario.Operador)
            return usuarioCriacaoId == UserId;

        // Admin vê:
        // - Seus próprios registros
        // - Registros de operadores
        // - NÃO vê de outros admins (a menos que seja proprietário)
        if (Perfil == PerfilUsuario.Admin)
        {
            // Se foi criado por ele mesmo
            if (usuarioCriacaoId == UserId)
                return true;

            // Se foi criado por operador, admin pode ver
            if (perfilCriador == PerfilUsuario.Operador)
                return true;

            // Não vê de outros admins
            return false;
        }

        return false;
    }
}
