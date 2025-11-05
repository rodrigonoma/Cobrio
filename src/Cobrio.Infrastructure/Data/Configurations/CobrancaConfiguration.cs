using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class CobrancaConfiguration : IEntityTypeConfiguration<Cobranca>
{
    public void Configure(EntityTypeBuilder<Cobranca> builder)
    {
        builder.ToTable("Cobranca");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(c => c.RegraCobrancaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(c => c.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(c => c.PayloadJson)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(c => c.DataVencimento)
            .IsRequired()
            .HasColumnType("DATE");

        builder.Property(c => c.DataDisparo)
            .IsRequired()
            .HasColumnType("DATE");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.TentativasEnvio)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.DataProcessamento)
            .HasColumnType("DATETIME");

        builder.Property(c => c.MensagemErro)
            .HasColumnType("TEXT");

        builder.Property(c => c.CriadoEm)
            .IsRequired();

        builder.Property(c => c.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(c => c.RegraCobranca)
            .WithMany(r => r.Cobrancas)
            .HasForeignKey(c => c.RegraCobrancaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.EmpresaCliente)
            .WithMany()
            .HasForeignKey(c => c.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Historicos)
            .WithOne(h => h.Cobranca)
            .HasForeignKey(h => h.CobrancaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => new { c.EmpresaClienteId, c.Status, c.DataDisparo })
            .HasDatabaseName("idx_cobranca_tenant_status_disparo");

        builder.HasIndex(c => new { c.RegraCobrancaId, c.Status })
            .HasDatabaseName("idx_cobranca_regra_status");

        builder.HasIndex(c => c.DataVencimento)
            .HasDatabaseName("idx_cobranca_data_vencimento");

        builder.HasIndex(c => new { c.Status, c.TentativasEnvio })
            .HasDatabaseName("idx_cobranca_status_tentativas");
    }
}
