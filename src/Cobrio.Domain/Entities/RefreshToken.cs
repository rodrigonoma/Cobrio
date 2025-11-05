namespace Cobrio.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UsuarioEmpresaId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string CreatedByIp { get; private set; } = null!;

    // Navegação
    public UsuarioEmpresa UsuarioEmpresa { get; private set; } = null!;

    private RefreshToken() { } // EF Core

    public RefreshToken(
        Guid usuarioEmpresaId,
        string token,
        DateTime expiresAt,
        string createdByIp)
    {
        Id = Guid.NewGuid();
        UsuarioEmpresaId = usuarioEmpresaId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        IsRevoked = false;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string ip)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token já foi revogado");

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ip;
    }
}
