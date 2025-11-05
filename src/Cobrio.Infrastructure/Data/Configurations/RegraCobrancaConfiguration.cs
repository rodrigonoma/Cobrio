using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class RegraCobrancaConfiguration : IEntityTypeConfiguration<RegraCobranca>
{
    public void Configure(EntityTypeBuilder<RegraCobranca> builder)
    {
        builder.ToTable("RegraCobranca");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(r => r.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(r => r.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Descricao)
            .HasMaxLength(1000);

        builder.Property(r => r.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.TipoMomento)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.ValorTempo)
            .IsRequired();

        builder.Property(r => r.UnidadeTempo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.CanalNotificacao)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.TemplateNotificacao)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(r => r.VariaveisObrigatorias)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(r => r.TokenWebhook)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(r => r.CriadoEm)
            .IsRequired();

        builder.Property(r => r.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(r => r.EmpresaCliente)
            .WithMany()
            .HasForeignKey(r => r.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Cobrancas)
            .WithOne(c => c.RegraCobranca)
            .HasForeignKey(c => c.RegraCobrancaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(r => new { r.EmpresaClienteId, r.Ativa })
            .HasDatabaseName("idx_regra_cobranca_tenant_ativa");

        builder.HasIndex(r => r.TokenWebhook)
            .IsUnique()
            .HasDatabaseName("idx_regra_cobranca_token");

        builder.HasIndex(r => new { r.EmpresaClienteId, r.CanalNotificacao })
            .HasDatabaseName("idx_regra_cobranca_tenant_canal");
    }
}
