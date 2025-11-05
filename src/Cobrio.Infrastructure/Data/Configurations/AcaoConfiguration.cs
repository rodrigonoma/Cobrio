using Cobrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class AcaoConfiguration : IEntityTypeConfiguration<Acao>
{
    public void Configure(EntityTypeBuilder<Acao> builder)
    {
        builder.ToTable("Acao");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(a => a.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Chave)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Descricao)
            .HasMaxLength(500);

        builder.Property(a => a.TipoAcao)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.CriadoEm)
            .IsRequired();

        builder.Property(a => a.AtualizadoEm)
            .IsRequired();

        // Índices
        builder.HasIndex(a => a.Chave)
            .IsUnique()
            .HasDatabaseName("uk_acao_chave");

        builder.HasIndex(a => a.TipoAcao)
            .HasDatabaseName("idx_acao_tipo");

        // Relacionamentos
        builder.HasMany(a => a.Permissoes)
            .WithOne(p => p.Acao)
            .HasForeignKey(p => p.AcaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
