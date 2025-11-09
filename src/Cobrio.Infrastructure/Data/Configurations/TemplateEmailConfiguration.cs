using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class TemplateEmailConfiguration : IEntityTypeConfiguration<TemplateEmail>
{
    public void Configure(EntityTypeBuilder<TemplateEmail> builder)
    {
        builder.ToTable("TemplateEmail");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(t => t.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(t => t.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Descricao)
            .HasMaxLength(1000);

        builder.Property(t => t.ConteudoHtml)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(t => t.SubjectEmail)
            .HasMaxLength(500);

        builder.Property(t => t.VariaveisObrigatorias)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(t => t.VariaveisObrigatoriasSistema)
            .HasColumnType("TEXT");

        builder.Property(t => t.CanalSugerido)
            .HasConversion<int>();

        builder.Property(t => t.CriadoEm)
            .IsRequired();

        builder.Property(t => t.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(t => t.EmpresaCliente)
            .WithMany()
            .HasForeignKey(t => t.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(t => t.EmpresaClienteId)
            .HasDatabaseName("idx_template_email_tenant");

        builder.HasIndex(t => new { t.EmpresaClienteId, t.Nome })
            .IsUnique()
            .HasDatabaseName("idx_template_email_tenant_nome");
    }
}
