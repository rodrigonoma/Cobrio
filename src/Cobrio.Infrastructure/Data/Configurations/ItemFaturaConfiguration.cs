using Cobrio.Domain.Entities;
using Cobrio.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class ItemFaturaConfiguration : IEntityTypeConfiguration<ItemFatura>
{
    public void Configure(EntityTypeBuilder<ItemFatura> builder)
    {
        builder.ToTable("ItemFatura");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(i => i.FaturaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(i => i.Descricao)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.Quantidade)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(i => i.ValorUnitario)
            .IsRequired()
            .HasColumnName("ValorUnitarioCentavos")
            .HasConversion(new MoneyConverter());

        builder.Property(i => i.ValorTotal)
            .IsRequired()
            .HasColumnName("ValorTotalCentavos")
            .HasConversion(new MoneyConverter());

        builder.Property(i => i.TipoItem)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValueSql("'Plano'");

        builder.Property(i => i.PlanoOfertaId)
            .IsRequired(false)
            .HasColumnType("CHAR(36)");

        builder.Property(i => i.CriadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(i => i.Fatura)
            .WithMany(f => f.Itens)
            .HasForeignKey(i => i.FaturaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.PlanoOferta)
            .WithMany()
            .HasForeignKey(i => i.PlanoOfertaId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(i => i.FaturaId)
            .HasDatabaseName("idx_fatura");
    }
}
