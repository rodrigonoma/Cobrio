using Cobrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class TentativaPagamentoConfiguration : IEntityTypeConfiguration<TentativaPagamento>
{
    public void Configure(EntityTypeBuilder<TentativaPagamento> builder)
    {
        builder.ToTable("TentativaPagamento");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(t => t.FaturaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(t => t.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(t => t.NumeroTentativa)
            .IsRequired();

        builder.Property(t => t.DataTentativa)
            .IsRequired();

        builder.Property(t => t.Resultado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.CodigoErro)
            .HasMaxLength(50);

        builder.Property(t => t.MensagemErro)
            .HasColumnType("TEXT");

        builder.Property(t => t.TransacaoIdGateway)
            .HasMaxLength(255);

        builder.Property(t => t.GatewayProvider)
            .HasMaxLength(50);

        builder.Property(t => t.CriadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(t => t.Fatura)
            .WithMany(f => f.Tentativas)
            .HasForeignKey(t => t.FaturaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.EmpresaCliente)
            .WithMany()
            .HasForeignKey(t => t.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(t => t.FaturaId)
            .HasDatabaseName("idx_fatura");

        builder.HasIndex(t => new { t.EmpresaClienteId, t.DataTentativa })
            .HasDatabaseName("idx_tenant_data");

        builder.HasIndex(t => t.Resultado)
            .HasDatabaseName("idx_resultado");
    }
}
