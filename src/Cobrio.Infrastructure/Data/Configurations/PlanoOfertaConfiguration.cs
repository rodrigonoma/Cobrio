using Cobrio.Domain.Entities;
using Cobrio.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class PlanoOfertaConfiguration : IEntityTypeConfiguration<PlanoOferta>
{
    public void Configure(EntityTypeBuilder<PlanoOferta> builder)
    {
        builder.ToTable("PlanoOferta");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(p => p.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Descricao)
            .HasColumnType("TEXT");

        builder.Property(p => p.TipoCiclo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Money Value Object
        builder.Property(p => p.Valor)
            .IsRequired()
            .HasColumnName("ValorCentavos")
            .HasConversion(new MoneyConverter());

        builder.Property<string>("_moeda")
            .HasColumnName("Moeda")
            .HasColumnType("CHAR(3)")
            .HasDefaultValue("BRL");

        builder.Property(p => p.PeriodoTrialDias)
            .HasColumnName("PeriodoTrial")
            .HasDefaultValue(0);

        builder.Property(p => p.LimiteUsuarios)
            .IsRequired(false);

        builder.Property(p => p.PermiteDowngrade)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.PermiteUpgrade)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.Property(p => p.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(p => p.EmpresaCliente)
            .WithMany(e => e.PlanosOferta)
            .HasForeignKey(p => p.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Assinantes)
            .WithOne(a => a.PlanoOferta)
            .HasForeignKey(a => a.PlanoOfertaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices multi-tenant
        builder.HasIndex(p => new { p.EmpresaClienteId, p.Ativo })
            .HasDatabaseName("idx_tenant_ativo");

        builder.HasIndex(p => new { p.EmpresaClienteId, p.Nome })
            .HasDatabaseName("idx_tenant_nome");
    }
}
