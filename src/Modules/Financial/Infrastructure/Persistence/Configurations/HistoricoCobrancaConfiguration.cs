using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Financial.Domain.Entities;

namespace Modules.Financial.Infrastructure.Persistence.Configurations;

public class HistoricoCobrancaConfiguration : IEntityTypeConfiguration<HistoricoCobranca>
{
    public void Configure(EntityTypeBuilder<HistoricoCobranca> builder)
    {
        builder.ToTable("HistoricosCobranca", "financial");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.MensagemEnviada)
            .HasMaxLength(1000);

        builder.Property(h => h.Observacao)
            .HasMaxLength(500);
    }
}
