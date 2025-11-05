using Cobrio.Domain.Entities;
using Cobrio.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class EmpresaClienteConfiguration : IEntityTypeConfiguration<EmpresaCliente>
{
    public void Configure(EntityTypeBuilder<EmpresaCliente> builder)
    {
        builder.ToTable("EmpresaCliente");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CNPJ)
            .IsRequired()
            .HasColumnType("CHAR(14)")
            .HasConversion(new CNPJConverter());

        builder.HasIndex(e => e.CNPJ)
            .IsUnique()
            .HasDatabaseName("idx_cnpj");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(new EmailConverter());

        builder.Property(e => e.Telefone)
            .HasMaxLength(20);

        builder.Property(e => e.PlanoCobrioId)
            .IsRequired();

        builder.Property(e => e.DataContrato)
            .IsRequired();

        builder.Property(e => e.StatusContrato)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Endereco (Owned Entity)
        builder.OwnsOne(e => e.Endereco, endereco =>
        {
            endereco.Property(a => a.Logradouro)
                .HasColumnName("Endereco_Logradouro")
                .HasMaxLength(200);

            endereco.Property(a => a.Numero)
                .HasColumnName("Endereco_Numero")
                .HasMaxLength(20);

            endereco.Property(a => a.Complemento)
                .HasColumnName("Endereco_Complemento")
                .HasMaxLength(100);

            endereco.Property(a => a.Bairro)
                .HasColumnName("Endereco_Bairro")
                .HasMaxLength(100);

            endereco.Property(a => a.Cidade)
                .HasColumnName("Endereco_Cidade")
                .HasMaxLength(100);

            endereco.Property(a => a.Estado)
                .HasColumnName("Endereco_Estado")
                .HasColumnType("CHAR(2)");

            endereco.Property(a => a.CEP)
                .HasColumnName("Endereco_CEP")
                .HasColumnType("CHAR(8)");

            endereco.Property(a => a.Pais)
                .HasColumnName("Endereco_Pais")
                .HasMaxLength(50)
                .HasDefaultValue("Brasil");
        });

        builder.Property(e => e.CriadoEm)
            .IsRequired();

        builder.Property(e => e.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasMany(e => e.Usuarios)
            .WithOne(u => u.EmpresaCliente)
            .HasForeignKey(u => u.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.PlanosOferta)
            .WithOne(p => p.EmpresaCliente)
            .HasForeignKey(p => p.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Assinantes)
            .WithOne(a => a.EmpresaCliente)
            .HasForeignKey(a => a.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ReguaDunning)
            .WithOne(r => r.EmpresaCliente)
            .HasForeignKey<ReguaDunningConfig>(r => r.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(e => e.StatusContrato)
            .HasDatabaseName("idx_status");

        builder.HasIndex(e => e.PlanoCobrioId)
            .HasDatabaseName("idx_plano");
    }
}
