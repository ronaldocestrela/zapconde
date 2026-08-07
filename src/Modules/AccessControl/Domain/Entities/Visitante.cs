using BuildingBlocks.Shared.MultiTenancy;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Domain.Exceptions;

namespace Modules.AccessControl.Domain.Entities;

/// <summary>
/// Entidade Aggregate Root que representa uma autorização ou registro de acesso de visitante/prestador de serviço.
/// Suporta isolamento multi-tenant via ITenantScoped.
/// </summary>
public class Visitante : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public string Documento { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public TipoVisitante Tipo { get; private set; }
    public StatusVisitante Status { get; private set; }
    public string? Empresa { get; private set; }
    public string? PlacaVeiculo { get; private set; }
    public int UnidadeId { get; private set; }
    public string BlocoUnidade { get; private set; } = string.Empty;
    public int? MoradorId { get; private set; }
    public DateTimeOffset? DataHoraInicioAutorizacao { get; private set; }
    public DateTimeOffset? DataHoraFimAutorizacao { get; private set; }
    public DateTimeOffset? DataHoraEntrada { get; private set; }
    public DateTimeOffset? DataHoraSaida { get; private set; }
    public string? Observacoes { get; private set; }
    public int? OperadorEntradaId { get; private set; }
    public int? OperadorSaidaId { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AtualizadoEm { get; private set; }

    // Construtor EF Core
    private Visitante() { }

    /// <summary>
    /// Factory Method para criar autorização/registro prévio de visitante ou prestador de serviço.
    /// </summary>
    public static Visitante CriarAutorizacao(
        int tenantId,
        int condoId,
        string nomeCompleto,
        string documento,
        string? telefone,
        TipoVisitante tipo,
        int unidadeId,
        string blocoUnidade,
        int? moradorId,
        DateTimeOffset? dataHoraInicioAutorizacao,
        DateTimeOffset? dataHoraFimAutorizacao,
        string? empresa,
        string? placaVeiculo,
        string? observacoes)
    {
        if (tenantId <= 0)
            throw new VisitanteDomainException("TenantId é obrigatório.");

        if (condoId <= 0)
            throw new VisitanteDomainException("CondoId é obrigatório.");

        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new VisitanteDomainException("O nome completo do visitante é obrigatório.");

        if (string.IsNullOrWhiteSpace(documento))
            throw new VisitanteDomainException("O documento de identificação (CPF/RG) é obrigatório.");

        if (unidadeId <= 0)
            throw new VisitanteDomainException("A unidade de destino é obrigatória.");

        if (tipo == TipoVisitante.PrestadorServico && string.IsNullOrWhiteSpace(empresa))
            throw new VisitanteDomainException("Nome da Empresa / Razão Social é obrigatória para Prestador de Serviço.");

        return new Visitante
        {
            TenantId = tenantId,
            CondoId = condoId,
            NomeCompleto = nomeCompleto.Trim(),
            Documento = documento.Trim(),
            Telefone = telefone?.Trim(),
            Tipo = tipo,
            Status = StatusVisitante.Agendado,
            UnidadeId = unidadeId,
            BlocoUnidade = string.IsNullOrWhiteSpace(blocoUnidade) ? $"Unidade {unidadeId}" : blocoUnidade.Trim(),
            MoradorId = moradorId,
            DataHoraInicioAutorizacao = dataHoraInicioAutorizacao ?? DateTimeOffset.UtcNow,
            DataHoraFimAutorizacao = dataHoraFimAutorizacao ?? DateTimeOffset.UtcNow.AddDays(1),
            Empresa = empresa?.Trim(),
            PlacaVeiculo = placaVeiculo?.Trim().ToUpperInvariant(),
            Observacoes = observacoes?.Trim(),
            CriadoEm = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Efetua a liberação e registro de entrada do visitante na portaria.
    /// </summary>
    public void RegistrarEntrada(int? operadorId = null)
    {
        if (Status == StatusVisitante.Presente)
            throw new VisitanteDomainException($"O visitante '{NomeCompleto}' já se encontra no condomínio.");

        if (Status == StatusVisitante.Finalizado || Status == StatusVisitante.Cancelado || Status == StatusVisitante.Negado)
            throw new VisitanteDomainException($"Não é possível registrar entrada para um cadastro com status {Status}.");

        Status = StatusVisitante.Presente;
        DataHoraEntrada = DateTimeOffset.UtcNow;
        OperadorEntradaId = operadorId;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Efetua o registro de saída do visitante na portaria.
    /// </summary>
    public void RegistrarSaida(int? operadorId = null)
    {
        if (Status != StatusVisitante.Presente)
            throw new VisitanteDomainException($"A saída só pode ser registrada para visitantes que estejam 'Presente' no condomínio. Status atual: {Status}.");

        Status = StatusVisitante.Finalizado;
        DataHoraSaida = DateTimeOffset.UtcNow;
        OperadorSaidaId = operadorId;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Cancela a autorização de entrada.
    /// </summary>
    public void CancelarAutorizacao(string? motivo = null)
    {
        if (Status == StatusVisitante.Finalizado)
            throw new VisitanteDomainException("Não é possível cancelar uma visita que já foi finalizada.");

        Status = StatusVisitante.Cancelado;
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            Observacoes = string.IsNullOrWhiteSpace(Observacoes) 
                ? $"[CANCELADO]: {motivo}" 
                : $"{Observacoes} | [CANCELADO]: {motivo}";
        }
        AtualizadoEm = DateTimeOffset.UtcNow;
    }
}
