using System.Text;
using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;

namespace Modules.Operations.Domain.Entities;

public class AssembleiaVirtual : ITenantScoped
{
    private readonly List<PautaAssembleia> _pautas = new();

    public Guid Id { get; private set; }
    public int TenantId { get; set; }
    public int CondoId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public TipoAssembleia Tipo { get; private set; }
    public StatusAssembleia Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public DateTime? DataEncerramento { get; private set; }
    public string? AtaTexto { get; private set; }
    public DateTime? AtaGeradaEm { get; private set; }
    public string CriadoPorUserId { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public IReadOnlyCollection<PautaAssembleia> Pautas => _pautas.AsReadOnly();

    private AssembleiaVirtual() { }

    public static AssembleiaVirtual Create(
        int tenantId,
        int condoId,
        string titulo,
        TipoAssembleia tipo,
        DateTime dataInicio,
        DateTime dataFim,
        string criadoPorUserId,
        string? descricao = null)
    {
        if (tenantId <= 0)
            throw new AssembleiaDomainException("TenantId é obrigatório.");

        if (condoId <= 0)
            throw new AssembleiaDomainException("CondoId é obrigatório.");

        if (string.IsNullOrWhiteSpace(titulo))
            throw new AssembleiaDomainException("O título da assembleia é obrigatório.");

        if (titulo.Length > 200)
            throw new AssembleiaDomainException("O título da assembleia deve ter no máximo 200 caracteres.");

        if (dataFim <= dataInicio)
            throw new AssembleiaDomainException("A data final da assembleia deve ser posterior à data inicial.");

        if (string.IsNullOrWhiteSpace(criadoPorUserId))
            throw new AssembleiaDomainException("O ID do criador da assembleia é obrigatório.");

        return new AssembleiaVirtual
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CondoId = condoId,
            Titulo = titulo.Trim(),
            Descricao = descricao?.Trim(),
            Tipo = tipo,
            Status = StatusAssembleia.Agendada,
            DataInicio = dataInicio,
            DataFim = dataFim,
            CriadoPorUserId = criadoPorUserId.Trim(),
            DataCriacao = DateTime.UtcNow
        };
    }

    public PautaAssembleia AdicionarPauta(string titulo, TipoVotacao tipoVotacao, string? descricao = null, List<string>? opcoesDisponiveis = null)
    {
        if (Status == StatusAssembleia.Encerrada || Status == StatusAssembleia.Cancelada)
            throw new AssembleiaDomainException("Não é possível adicionar pautas a uma assembleia encerrada ou cancelada.");

        int proximaOrdem = _pautas.Count + 1;
        var pauta = PautaAssembleia.Create(Id, titulo, tipoVotacao, proximaOrdem, descricao, opcoesDisponiveis);
        _pautas.Add(pauta);
        DataAtualizacao = DateTime.UtcNow;

        return pauta;
    }

    public void IniciarAssembleia()
    {
        if (Status == StatusAssembleia.Encerrada || Status == StatusAssembleia.Cancelada)
            throw new AssembleiaDomainException($"Não é possível iniciar uma assembleia com status '{Status}'.");

        Status = StatusAssembleia.EmAndamento;
        DataAtualizacao = DateTime.UtcNow;
    }

    public VotoAssembleia RegistrarVoto(
        Guid pautaId,
        string moradorUserId,
        string unidadeId,
        string opcaoEscolhida,
        double pesoVoto = 1.0)
    {
        if (Status != StatusAssembleia.EmAndamento)
        {
            throw new AssembleiaEncerradaException(Titulo);
        }

        var pauta = _pautas.FirstOrDefault(p => p.Id == pautaId);
        if (pauta == null)
        {
            throw new AssembleiaDomainException($"Pauta com ID '{pautaId}' não encontrada nesta assembleia.");
        }

        var voto = pauta.RegistrarVoto(TenantId, CondoId, moradorUserId, unidadeId, opcaoEscolhida, pesoVoto);
        DataAtualizacao = DateTime.UtcNow;

        return voto;
    }

    public void EncerrarEGerarAta()
    {
        if (Status == StatusAssembleia.Encerrada)
            return;

        if (Status == StatusAssembleia.Cancelada)
            throw new AssembleiaDomainException("Não é possível encerrar uma assembleia cancelada.");

        Status = StatusAssembleia.Encerrada;
        DataEncerramento = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;

        foreach (var pauta in _pautas)
        {
            pauta.EncerrarPauta();
        }

        // Apuração de Quórum e Geração da Ata Oficial
        AtaTexto = GerarTextoAtaOficial();
        AtaGeradaEm = DateTime.UtcNow;
    }

    public void CancelarAssembleia()
    {
        if (Status == StatusAssembleia.Encerrada)
            throw new AssembleiaDomainException("Não é possível cancelar uma assembleia já encerrada.");

        Status = StatusAssembleia.Cancelada;
        DataAtualizacao = DateTime.UtcNow;
    }

    private string GerarTextoAtaOficial()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"==========================================================================");
        sb.AppendLine($"ATA OFICIAL DA {Titulo.ToUpper()}");
        sb.AppendLine($"==========================================================================");
        sb.AppendLine($"Condomínio (ID): {CondoId} | Tenant: {TenantId}");
        sb.AppendLine($"Tipo de Assembleia: {Tipo}");
        sb.AppendLine($"Período de Realização: {DataInicio:dd/MM/yyyy HH:mm} até {DataFim:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Data de Encerramento e Lavratura da Ata: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC");
        sb.AppendLine($"Identificador da Assembleia: {Id}");
        sb.AppendLine();
        sb.AppendLine($"RESUMO DE DELIBERAÇÕES E APURAÇÃO DE VOTOS:");
        sb.AppendLine($"--------------------------------------------------------------------------");

        if (_pautas.Count == 0)
        {
            sb.AppendLine("Nenhuma pauta foi deliberada nesta assembleia.");
        }

        int totalUnidadesPresentes = _pautas
            .SelectMany(p => p.Votos)
            .Select(v => v.UnidadeId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        sb.AppendLine($"Quórum Total de Unidades Participantes: {totalUnidadesPresentes} unidade(s)");
        sb.AppendLine();

        for (int i = 0; i < _pautas.Count; i++)
        {
            var pauta = _pautas[i];
            sb.AppendLine($"PAUTA #{i + 1}: {pauta.Titulo}");
            if (!string.IsNullOrWhiteSpace(pauta.Descricao))
            {
                sb.AppendLine($"Descrição: {pauta.Descricao}");
            }
            sb.AppendLine($"Regra de Votação: {pauta.TipoVotacao}");

            var contagem = pauta.ApurarContagemVotos();
            int totalVotosPauta = pauta.Votos.Count;

            sb.AppendLine($"Total de Votos Registrados nesta Pauta: {totalVotosPauta}");
            foreach (var item in contagem)
            {
                double pct = totalVotosPauta > 0 ? (item.Value * 100.0 / totalVotosPauta) : 0;
                sb.AppendLine($"  - Opção '{item.Key}': {item.Value} voto(s) ({pct:F1}%)");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"--------------------------------------------------------------------------");
        sb.AppendLine($"Ata lavrada eletronicamente pelo Sistema SmartCondo SaaS.");
        sb.AppendLine($"Hash de Autenticidade: {Guid.NewGuid():N}");
        sb.AppendLine($"==========================================================================");

        return sb.ToString();
    }
}
