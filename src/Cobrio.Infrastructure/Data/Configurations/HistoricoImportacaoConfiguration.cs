using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class HistoricoImportacaoConfiguration : IEntityTypeConfiguration<HistoricoImportacao>
{
    public void Configure(EntityTypeBuilder<HistoricoImportacao> builder)
    {
        builder.ToTable("HistoricoImportacao");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(h => h.RegraCobrancaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(h => h.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(h => h.UsuarioId)
            .HasColumnType("CHAR(36)");

        builder.Property(h => h.NomeArquivo)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("VARCHAR(255)");

        builder.Property(h => h.DataImportacao)
            .IsRequired()
            .HasColumnType("DATETIME");

        builder.Property(h => h.TotalLinhas)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.LinhasProcessadas)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.LinhasComErro)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.ErrosJson)
            .HasColumnType("LONGTEXT");

        builder.Property(h => h.CriadoEm)
            .IsRequired();

        builder.Property(h => h.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(h => h.RegraCobranca)
            .WithMany()
            .HasForeignKey(h => h.RegraCobrancaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.EmpresaCliente)
            .WithMany()
            .HasForeignKey(h => h.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(h => new { h.EmpresaClienteId, h.DataImportacao })
            .HasDatabaseName("idx_historico_importacao_tenant_data");

        builder.HasIndex(h => new { h.RegraCobrancaId, h.DataImportacao })
            .HasDatabaseName("idx_historico_importacao_regra_data");

        builder.HasIndex(h => h.Status)
            .HasDatabaseName("idx_historico_importacao_status");
    }
}
