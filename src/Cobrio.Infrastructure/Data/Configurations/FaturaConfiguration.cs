using Cobrio.Domain.Entities;
using Cobrio.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cobrio.Infrastructure.Data.Configurations;

public class FaturaConfiguration : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("Fatura");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(f => f.AssinanteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(f => f.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(f => f.NumeroFatura)
            .IsRequired()
            .HasMaxLength(50);

        // Money Value Objects
        builder.Property(f => f.ValorBruto)
            .IsRequired()
            .HasColumnName("ValorBrutoCentavos")
            .HasConversion(new MoneyConverter());

        builder.Property(f => f.Desconto)
            .IsRequired()
            .HasColumnName("DescontoCentavos")
            .HasConversion(new MoneyConverter())
            .HasDefaultValueSql("0");

        builder.Property(f => f.Impostos)
            .IsRequired()
            .HasColumnName("ImpostosCentavos")
            .HasConversion(new MoneyConverter())
            .HasDefaultValueSql("0");

        builder.Property(f => f.ValorLiquido)
            .IsRequired()
            .HasColumnName("ValorLiquidoCentavos")
            .HasConversion(new MoneyConverter());

        builder.Property<string>("_moeda")
            .HasColumnName("Moeda")
            .HasColumnType("CHAR(3)")
            .HasDefaultValue("BRL");

        builder.Property(f => f.DataEmissao)
            .IsRequired();

        builder.Property(f => f.DataVencimento)
            .IsRequired();

        builder.Property(f => f.DataPagamento)
            .IsRequired(false);

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValueSql("'Pendente'");

        builder.Property(f => f.MetodoPagamentoId)
            .IsRequired(false)
            .HasColumnType("CHAR(36)");

        builder.Property(f => f.TransacaoIdGateway)
            .HasMaxLength(255);

        builder.Property(f => f.Observacoes)
            .HasColumnType("TEXT");

        builder.Property(f => f.LinkBoleto)
            .HasColumnType("TEXT");

        builder.Property(f => f.QrCodePix)
            .HasColumnType("TEXT");

        builder.Property(f => f.CriadoEm)
            .IsRequired();

        builder.Property(f => f.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(f => f.Assinante)
            .WithMany(a => a.Faturas)
            .HasForeignKey(f => f.AssinanteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.EmpresaCliente)
            .WithMany()
            .HasForeignKey(f => f.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.MetodoPagamento)
            .WithMany()
            .HasForeignKey(f => f.MetodoPagamentoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(f => f.Itens)
            .WithOne(i => i.Fatura)
            .HasForeignKey(i => i.FaturaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Tentativas)
            .WithOne(t => t.Fatura)
            .HasForeignKey(t => t.FaturaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices multi-tenant
        builder.HasIndex(f => new { f.EmpresaClienteId, f.NumeroFatura })
            .IsUnique()
            .HasDatabaseName("uk_numero_fatura");

        builder.HasIndex(f => new { f.EmpresaClienteId, f.Status })
            .HasDatabaseName("idx_tenant_status");

        builder.HasIndex(f => new { f.EmpresaClienteId, f.DataVencimento })
            .HasDatabaseName("idx_tenant_vencimento");

        builder.HasIndex(f => new { f.AssinanteId, f.Status })
            .HasDatabaseName("idx_assinante_status");

        builder.HasIndex(f => f.DataVencimento)
            .HasDatabaseName("idx_data_vencimento");

        builder.HasIndex(f => new { f.Status, f.DataVencimento })
            .HasDatabaseName("idx_status_vencimento");
    }
}
