using Cobrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Cobrio.Infrastructure.Data.Configurations;

public class ReguaDunningConfigConfiguration : IEntityTypeConfiguration<ReguaDunningConfig>
{
    public void Configure(EntityTypeBuilder<ReguaDunningConfig> builder)
    {
        builder.ToTable("ReguaDunningConfig");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(r => r.EmpresaClienteId)
            .IsRequired()
            .HasColumnType("CHAR(36)");

        builder.Property(r => r.NumeroMaximoTentativas)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(r => r.IntervalosDias)
            .IsRequired()
            .HasColumnName("IntervalosDiasJson")
            .HasColumnType("JSON")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int> { 1, 3, 7 });

        builder.Property(r => r.EnviarEmail)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.EnviarSMS)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.EnviarNotificacaoInApp)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.DiasSuspensao)
            .IsRequired()
            .HasDefaultValue(15);

        builder.Property(r => r.DiasCancelamento)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(r => r.HoraInicioRetry)
            .IsRequired()
            .HasDefaultValue(new TimeSpan(8, 0, 0));

        builder.Property(r => r.HoraFimRetry)
            .IsRequired()
            .HasDefaultValue(new TimeSpan(20, 0, 0));

        builder.Property(r => r.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.CriadoEm)
            .IsRequired();

        builder.Property(r => r.AtualizadoEm)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(r => r.EmpresaCliente)
            .WithOne(e => e.ReguaDunning)
            .HasForeignKey<ReguaDunningConfig>(r => r.EmpresaClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único
        builder.HasIndex(r => r.EmpresaClienteId)
            .IsUnique();
    }
}
