using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class ContaBancariaConfiguration : IEntityTypeConfiguration<ContaBancaria>
{
    public void Configure(EntityTypeBuilder<ContaBancaria> builder)
    {
        builder.ToTable("ContasBancarias", "financial");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NomeBanco)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CodigoBanco)
            .HasMaxLength(10);

        builder.Property(c => c.Agencia)
            .HasMaxLength(20);

        builder.Property(c => c.NumeroConta)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.SaldoAtual)
            .HasPrecision(18, 2);
    }
}
