using Cobrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(r => r.UsuarioEmpresaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        builder.Property(r => r.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.RevokedAt)
            .IsRequired(false);

        builder.Property(r => r.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(r => r.CreatedByIp)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.CriadoEm)
            .IsRequired();

        builder.Property(r => r.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(r => r.UsuarioEmpresa)
            .WithMany()
            .HasForeignKey(r => r.UsuarioEmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(r => r.Token)
            .IsUnique()
            .HasDatabaseName("idx_token");

        builder.HasIndex(r => new { r.UsuarioEmpresaId, r.IsRevoked, r.ExpiresAt })
            .HasDatabaseName("idx_usuario_ativo");
    }
}
