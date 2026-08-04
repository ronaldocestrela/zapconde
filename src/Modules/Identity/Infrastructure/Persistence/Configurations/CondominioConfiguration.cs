using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class CondominioConfiguration : IEntityTypeConfiguration<Condominio>
{
    public void Configure(EntityTypeBuilder<Condominio> builder)
    {
        builder.ToTable("condominios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.MasterAdminName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CorporateEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PhoneWhatsApp).HasMaxLength(32);
        builder.Property(x => x.EmergencyPhone).HasMaxLength(32);

        builder.OwnsOne(x => x.Endereco, endereco =>
        {
            endereco.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(8);
            endereco.Property(e => e.Logradouro).HasColumnName("logradouro").HasMaxLength(256);
            endereco.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(32);
            endereco.Property(e => e.Bairro).HasColumnName("bairro").HasMaxLength(128);
            endereco.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(128);
            endereco.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
        });

        builder.OwnsOne(x => x.Configuracoes, config =>
        {
            config.Property(c => c.DiaVencimento).HasColumnName("dia_vencimento");
            config.Property(c => c.JurosEnabled).HasColumnName("juros_enabled");
            config.Property(c => c.MultaEnabled).HasColumnName("multa_enabled");
            config.Property(c => c.BankGateway).HasColumnName("bank_gateway").HasConversion<string>().HasMaxLength(32);
            config.Property(c => c.WhatsAppAiEnabled).HasColumnName("whatsapp_ai_enabled");
        });

        builder.HasIndex(x => new { x.TenantId, x.Nome });
    }
}
