using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Domain.Exceptions;

namespace Modules.Operations.Domain.Entities;

public class VotoAssembleia : ITenantScoped
{
    public Guid Id { get; private set; }
    public int TenantId { get; set; }
    public int CondoId { get; private set; }
    public Guid AssembleiaId { get; private set; }
    public Guid PautaId { get; private set; }
    public string MoradorUserId { get; private set; } = string.Empty;
    public string UnidadeId { get; private set; } = string.Empty;
    public string OpcaoEscolhida { get; private set; } = string.Empty;
    public double PesoVoto { get; private set; } = 1.0;
    public DateTime DataVoto { get; private set; }

    private VotoAssembleia() { }

    public static VotoAssembleia Create(
        int tenantId,
        int condoId,
        Guid assembleiaId,
        Guid pautaId,
        string moradorUserId,
        string unidadeId,
        string opcaoEscolhida,
        double pesoVoto = 1.0,
        Guid? id = null)
    {
        if (tenantId <= 0)
            throw new AssembleiaDomainException("TenantId é obrigatório.");

        if (condoId <= 0)
            throw new AssembleiaDomainException("CondoId é obrigatório.");

        if (assembleiaId == Guid.Empty)
            throw new AssembleiaDomainException("AssembleiaId é obrigatório.");

        if (pautaId == Guid.Empty)
            throw new AssembleiaDomainException("PautaId é obrigatório.");

        if (string.IsNullOrWhiteSpace(moradorUserId))
            throw new AssembleiaDomainException("MoradorUserId é obrigatório.");

        if (string.IsNullOrWhiteSpace(unidadeId))
            throw new AssembleiaDomainException("UnidadeId é obrigatória.");

        if (string.IsNullOrWhiteSpace(opcaoEscolhida))
            throw new AssembleiaDomainException("A opção de voto escolhida é obrigatória.");

        if (pesoVoto <= 0)
            throw new AssembleiaDomainException("O peso do voto deve ser maior que zero.");

        return new VotoAssembleia
        {
            Id = id ?? Guid.Empty,
            TenantId = tenantId,
            CondoId = condoId,
            AssembleiaId = assembleiaId,
            PautaId = pautaId,
            MoradorUserId = moradorUserId.Trim(),
            UnidadeId = unidadeId.Trim(),
            OpcaoEscolhida = opcaoEscolhida.Trim(),
            PesoVoto = pesoVoto,
            DataVoto = DateTime.UtcNow
        };
    }
}
