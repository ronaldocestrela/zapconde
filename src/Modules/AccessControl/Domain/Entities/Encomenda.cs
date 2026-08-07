using BuildingBlocks.Shared.MultiTenancy;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Domain.Exceptions;

namespace Modules.AccessControl.Domain.Entities;

/// <summary>
/// Entidade Aggregate Root que representa o recebimento e guarda de encomendas na portaria.
/// Possui isolamento multi-tenant via ITenantScoped.
/// </summary>
public class Encomenda : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public int UnidadeId { get; private set; }
    public string BlocoUnidade { get; private set; } = string.Empty;
    public string CodigoRastreio { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string? Remetente { get; private set; }
    public string? Transportadora { get; private set; }
    public string? LocalArmazenamento { get; private set; }
    public TipoEncomenda Tipo { get; private set; }
    public StatusEncomenda Status { get; private set; }
    public DateTimeOffset DataRecebimento { get; private set; }
    public string RecebidoPorNome { get; private set; } = string.Empty;
    public DateTimeOffset? DataRetirada { get; private set; }
    public string? RetiradoPorNome { get; private set; }
    public DateTimeOffset? NotificadoEm { get; private set; }
    public string? Observacoes { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AtualizadoEm { get; private set; }

    // Construtor EF Core
    private Encomenda() { }

    /// <summary>
    /// Factory Method para registrar o recebimento de uma nova encomenda na portaria.
    /// </summary>
    public static Encomenda Criar(
        int tenantId,
        int condoId,
        int unidadeId,
        string blocoUnidade,
        string codigoRastreio,
        string descricao,
        string? remetente,
        string? transportadora,
        string? localArmazenamento,
        TipoEncomenda tipo,
        string recebidoPorNome,
        DateTimeOffset dataRecebimento,
        string? observacoes = null)
    {
        if (tenantId <= 0)
            throw new EncomendaDomainException("TenantId é obrigatório e deve ser maior que zero.");

        if (condoId <= 0)
            throw new EncomendaDomainException("CondoId é obrigatório e deve ser maior que zero.");

        if (unidadeId <= 0)
            throw new EncomendaDomainException("UnidadeId é obrigatória e deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(blocoUnidade))
            throw new EncomendaDomainException("Identificação da unidade/bloco é obrigatória.");

        if (string.IsNullOrWhiteSpace(codigoRastreio) && string.IsNullOrWhiteSpace(descricao))
            throw new EncomendaDomainException("O código de rastreio ou uma descrição da encomenda deve ser informada.");

        if (string.IsNullOrWhiteSpace(recebidoPorNome))
            throw new EncomendaDomainException("Nome do responsável pelo recebimento na portaria é obrigatório.");

        if (dataRecebimento > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new EncomendaDomainException("Data e hora de recebimento não pode estar no futuro.");

        return new Encomenda
        {
            TenantId = tenantId,
            CondoId = condoId,
            UnidadeId = unidadeId,
            BlocoUnidade = blocoUnidade.Trim(),
            CodigoRastreio = (codigoRastreio ?? string.Empty).Trim(),
            Descricao = (descricao ?? string.Empty).Trim(),
            Remetente = remetente?.Trim(),
            Transportadora = transportadora?.Trim(),
            LocalArmazenamento = localArmazenamento?.Trim(),
            Tipo = tipo,
            Status = StatusEncomenda.AguardandoRetirada,
            DataRecebimento = dataRecebimento,
            RecebidoPorNome = recebidoPorNome.Trim(),
            Observacoes = observacoes?.Trim(),
            CriadoEm = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Registra a retirada/baixa da encomenda pelo morador ou pessoa autorizada.
    /// </summary>
    public void MarcarComoEntregue(string retiradoPorNome, DateTimeOffset dataRetirada)
    {
        if (Status != StatusEncomenda.AguardandoRetirada)
            throw new EncomendaDomainException($"Não é possível entregar uma encomenda com status '{Status}'. Apenas encomendas aguardando retirada podem ser entregues.");

        if (string.IsNullOrWhiteSpace(retiradoPorNome))
            throw new EncomendaDomainException("O nome da pessoa que retirou a encomenda é obrigatório.");

        if (dataRetirada < DataRecebimento)
            throw new EncomendaDomainException("A data de retirada não pode ser anterior à data de recebimento da encomenda.");

        if (dataRetirada > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new EncomendaDomainException("A data de retirada não pode estar no futuro.");

        Status = StatusEncomenda.Entregue;
        RetiradoPorNome = retiradoPorNome.Trim();
        DataRetirada = dataRetirada;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Notifica o morador sobre a disponibilidade da encomenda para retirada.
    /// </summary>
    public void NotificarMorador()
    {
        if (Status != StatusEncomenda.AguardandoRetirada)
            throw new EncomendaDomainException("Apenas encomendas pendentes de retirada podem ter notificação enviada.");

        NotificadoEm = DateTimeOffset.UtcNow;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Cancela o registro da encomenda na portaria.
    /// </summary>
    public void Cancelar(string motivo)
    {
        if (Status == StatusEncomenda.Entregue)
            throw new EncomendaDomainException("Encomendas já entregues não podem ser canceladas.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new EncomendaDomainException("É obrigatório informar um motivo para o cancelamento.");

        Status = StatusEncomenda.Cancelada;
        Observacoes = string.IsNullOrWhiteSpace(Observacoes) 
            ? $"[CANCELADA]: {motivo.Trim()}" 
            : $"{Observacoes} | [CANCELADA]: {motivo.Trim()}";
        AtualizadoEm = DateTimeOffset.UtcNow;
    }
}
