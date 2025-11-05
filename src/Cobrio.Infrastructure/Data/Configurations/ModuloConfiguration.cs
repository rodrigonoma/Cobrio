using Cobrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class ModuloConfiguration : IEntityTypeConfiguration<Modulo>
{
    public void Configure(EntityTypeBuilder<Modulo> builder)
    {
        builder.ToTable("Modulo");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(m => m.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Chave)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Descricao)
            .HasMaxLength(500);

        builder.Property(m => m.Icone)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("pi-circle");

        builder.Property(m => m.Rota)
            .HasMaxLength(200);

        builder.Property(m => m.Ordem)
            .IsRequired();

        builder.Property(m => m.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CriadoEm)
            .IsRequired();

        builder.Property(m => m.AtualizadoEm)
            .IsRequired();

        // Índices
        builder.HasIndex(m => m.Chave)
            .IsUnique()
            .HasDatabaseName("uk_modulo_chave");

        builder.HasIndex(m => m.Ordem)
            .HasDatabaseName("idx_modulo_ordem");

        // Relacionamentos
        builder.HasMany(m => m.Permissoes)
            .WithOne(p => p.Modulo)
            .HasForeignKey(p => p.ModuloId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
