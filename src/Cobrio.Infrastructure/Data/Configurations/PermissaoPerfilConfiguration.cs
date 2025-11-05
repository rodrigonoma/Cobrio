using Cobrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class PermissaoPerfilConfiguration : IEntityTypeConfiguration<PermissaoPerfil>
{
    public void Configure(EntityTypeBuilder<PermissaoPerfil> builder)
    {
        builder.ToTable("PermissaoPerfil");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(p => p.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(p => p.PerfilUsuario)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.ModuloId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(p => p.AcaoId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(p => p.Permitido)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CriadoPorUsuarioId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.Property(p => p.AtualizadoEm)
            .IsRequired();

        // Índices
        builder.HasIndex(p => new { p.EmpresaClienteId, p.PerfilUsuario, p.ModuloId, p.AcaoId })
            .IsUnique()
            .HasDatabaseName("uk_permissao_empresa_perfil_modulo_acao");

        builder.HasIndex(p => new { p.EmpresaClienteId, p.PerfilUsuario })
            .HasDatabaseName("idx_permissao_empresa_perfil");

        // Relacionamentos
        builder.HasOne(p => p.EmpresaCliente)
            .WithMany()
            .HasForeignKey(p => p.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Modulo)
            .WithMany(m => m.Permissoes)
            .HasForeignKey(p => p.ModuloId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Acao)
            .WithMany(a => a.Permissoes)
            .HasForeignKey(p => p.AcaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CriadoPor)
            .WithMany()
            .HasForeignKey(p => p.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
