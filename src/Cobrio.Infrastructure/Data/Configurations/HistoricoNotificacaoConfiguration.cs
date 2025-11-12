using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class HistoricoNotificacaoConfiguration : IEntityTypeConfiguration<HistoricoNotificacao>
{
    public void Configure(EntityTypeBuilder<HistoricoNotificacao> builder)
    {
        builder.ToTable("HistoricoNotificacao");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(h => h.CobrancaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(h => h.RegraCobrancaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(h => h.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(h => h.CanalUtilizado)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.MensagemEnviada)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(h => h.MensagemErro)
            .HasColumnType("TEXT");

        builder.Property(h => h.DataEnvio)
            .IsRequired()
            .HasColumnType("DATETIME");

        builder.Property(h => h.PayloadUtilizado)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(h => h.RespostaProvedor)
            .HasColumnType("TEXT");

        builder.Property(h => h.CriadoEm)
            .IsRequired();

        builder.Property(h => h.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(h => h.Cobranca)
            .WithMany(c => c.Historicos)
            .HasForeignKey(h => h.CobrancaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.RegraCobranca)
            .WithMany()
            .HasForeignKey(h => h.RegraCobrancaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(h => h.EmpresaCliente)
            .WithMany()
            .HasForeignKey(h => h.EmpresaClienteId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relacionamento com HistoricoStatusNotificacao
        builder.HasMany(h => h.HistoricoStatus)
            .WithOne(s => s.HistoricoNotificacao)
            .HasForeignKey(s => s.HistoricoNotificacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(h => new { h.EmpresaClienteId, h.DataEnvio })
            .HasDatabaseName("idx_historico_tenant_data");

        builder.HasIndex(h => new { h.CobrancaId, h.Status })
            .HasDatabaseName("idx_historico_cobranca_status");

        builder.HasIndex(h => new { h.RegraCobrancaId, h.Status })
            .HasDatabaseName("idx_historico_regra_status");

        builder.HasIndex(h => new { h.Status, h.DataEnvio })
            .HasDatabaseName("idx_historico_status_data");

        builder.HasIndex(h => new { h.CanalUtilizado, h.Status })
            .HasDatabaseName("idx_historico_canal_status");
    }
}
