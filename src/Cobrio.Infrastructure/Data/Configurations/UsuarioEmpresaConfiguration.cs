using Cobrio.Domain.Entities;
using Cobrio.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class UsuarioEmpresaConfiguration : IEntityTypeConfiguration<UsuarioEmpresa>
{
    public void Configure(EntityTypeBuilder<UsuarioEmpresa> builder)
    {
        builder.ToTable("UsuarioEmpresa");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(u => u.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(new EmailConverter());

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Perfil)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValueSql("'Operador'");

        builder.Property(u => u.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.EhProprietario)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.UltimoAcesso)
            .IsRequired(false);

        builder.Property(u => u.CriadoEm)
            .IsRequired();

        builder.Property(u => u.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(u => u.EmpresaCliente)
            .WithMany(e => e.Usuarios)
            .HasForeignKey(u => u.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(u => new { u.Email, u.EmpresaClienteId })
            .IsUnique()
            .HasDatabaseName("uk_email_empresa");

        builder.HasIndex(u => new { u.EmpresaClienteId, u.Email })
            .HasDatabaseName("idx_tenant_email");

        builder.HasIndex(u => new { u.EmpresaClienteId, u.Ativo })
            .HasDatabaseName("idx_tenant_ativo");
    }
}
