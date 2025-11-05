using Cobrio.Domain.Entities;
using Cobrio.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class AssinanteConfiguration : IEntityTypeConfiguration<Assinante>
{
    public void Configure(EntityTypeBuilder<Assinante> builder)
    {
        builder.ToTable("Assinante");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(a => a.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(a => a.PlanoOfertaId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(a => a.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(new EmailConverter());

        builder.Property(a => a.CPFCNPJ)
            .HasMaxLength(14);

        builder.Property(a => a.Telefone)
            .HasMaxLength(20);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.DataInicio)
            .IsRequired();

        builder.Property(a => a.DataFimCiclo)
            .IsRequired();

        builder.Property(a => a.DataCancelamento)
            .IsRequired(false);

        builder.Property(a => a.MotivoCancelamento)
            .HasColumnType("TEXT");

        builder.Property(a => a.EmTrial)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.DataFimTrial)
            .IsRequired(false);

        builder.Property(a => a.DiaVencimento)
            .IsRequired();

        builder.Property(a => a.ProximaCobranca)
            .IsRequired();

        // Endereco (Owned Entity)
        builder.OwnsOne(a => a.Endereco, endereco =>
        {
            endereco.Property(e => e.Logradouro)
                .HasColumnName("Endereco_Logradouro")
                .HasMaxLength(200);

            endereco.Property(e => e.Numero)
                .HasColumnName("Endereco_Numero")
                .HasMaxLength(20);

            endereco.Property(e => e.Complemento)
                .HasColumnName("Endereco_Complemento")
                .HasMaxLength(100);

            endereco.Property(e => e.Bairro)
                .HasColumnName("Endereco_Bairro")
                .HasMaxLength(100);

            endereco.Property(e => e.Cidade)
                .HasColumnName("Endereco_Cidade")
                .HasMaxLength(100);

            endereco.Property(e => e.Estado)
                .HasColumnName("Endereco_Estado")
                .HasColumnType("CHAR(2)");

            endereco.Property(e => e.CEP)
                .HasColumnName("Endereco_CEP")
                .HasColumnType("CHAR(8)");

            endereco.Property(e => e.Pais)
                .HasColumnName("Endereco_Pais")
                .HasMaxLength(50)
                .HasDefaultValue("Brasil");
        });

        builder.Property(a => a.CriadoEm)
            .IsRequired();

        builder.Property(a => a.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(a => a.EmpresaCliente)
            .WithMany(e => e.Assinantes)
            .HasForeignKey(a => a.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.PlanoOferta)
            .WithMany(p => p.Assinantes)
            .HasForeignKey(a => a.PlanoOfertaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.MetodosPagamento)
            .WithOne(m => m.Assinante)
            .HasForeignKey(m => m.AssinanteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Faturas)
            .WithOne(f => f.Assinante)
            .HasForeignKey(f => f.AssinanteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices multi-tenant otimizados
        builder.HasIndex(a => new { a.EmpresaClienteId, a.Status })
            .HasDatabaseName("idx_tenant_status");

        builder.HasIndex(a => new { a.EmpresaClienteId, a.Email })
            .HasDatabaseName("idx_tenant_email");

        builder.HasIndex(a => new { a.EmpresaClienteId, a.ProximaCobranca })
            .HasDatabaseName("idx_tenant_proxima_cobranca");

        builder.HasIndex(a => a.ProximaCobranca)
            .HasDatabaseName("idx_proxima_cobranca");

        builder.HasIndex(a => a.PlanoOfertaId)
            .HasDatabaseName("idx_plano");
    }
}
