using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class HistoricoStatusNotificacaoConfiguration : IEntityTypeConfiguration<HistoricoStatusNotificacao>
{
    public void Configure(EntityTypeBuilder<HistoricoStatusNotificacao> builder)
    {
        builder.ToTable("HistoricoStatusNotificacao");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(h => h.HistoricoNotificacaoId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(h => h.StatusAnterior)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.StatusNovo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.DataMudanca)
            .IsRequired()
            .HasColumnType("DATETIME");

        builder.Property(h => h.Detalhes)
            .HasColumnType("TEXT");

        builder.Property(h => h.IpOrigem)
            .HasMaxLength(50);

        builder.Property(h => h.UserAgent)
            .HasMaxLength(500);

        builder.Property(h => h.CriadoEm)
            .IsRequired();

        builder.Property(h => h.AtualizadoEm)
            .IsRequired();

        // Relacionamento já configurado no HistoricoNotificacaoConfiguration

        // Índices
        builder.HasIndex(h => new { h.HistoricoNotificacaoId, h.DataMudanca })
            .HasDatabaseName("idx_historico_status_data");

        builder.HasIndex(h => h.DataMudanca)
            .HasDatabaseName("idx_historico_status_mudanca");
    }
}
