using Cobrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class MetodoPagamentoConfiguration : IEntityTypeConfiguration<MetodoPagamento>
{
    public void Configure(EntityTypeBuilder<MetodoPagamento> builder)
    {
        builder.ToTable("MetodoPagamento");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(m => m.AssinanteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(m => m.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(m => m.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(m => m.TokenGateway)
            .HasMaxLength(255);

        builder.Property(m => m.GatewayProvider)
            .HasMaxLength(50);

        builder.Property(m => m.UltimosDigitos)
            .HasMaxLength(4);

        builder.Property(m => m.Bandeira)
            .HasMaxLength(30);

        builder.Property(m => m.NomeTitular)
            .HasMaxLength(150);

        builder.Property(m => m.DataValidade)
            .IsRequired(false);

        builder.Property(m => m.Principal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CriadoEm)
            .IsRequired();

        builder.Property(m => m.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(m => m.Assinante)
            .WithMany(a => a.MetodosPagamento)
            .HasForeignKey(m => m.AssinanteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.EmpresaCliente)
            .WithMany()
            .HasForeignKey(m => m.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(m => m.AssinanteId)
            .HasDatabaseName("idx_assinante");

        builder.HasIndex(m => new { m.EmpresaClienteId, m.Ativo })
            .HasDatabaseName("idx_tenant_ativo");

        builder.HasIndex(m => new { m.AssinanteId, m.Principal })
            .HasDatabaseName("idx_assinante_principal");
    }
}
