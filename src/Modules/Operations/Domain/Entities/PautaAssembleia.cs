using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;

namespace Modules.Operations.Domain.Entities;

public class PautaAssembleia
{
    private readonly List<VotoAssembleia> _votos = new();

    public Guid Id { get; private set; }
    public Guid AssembleiaId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int Ordem { get; private set; }
    public TipoVotacao TipoVotacao { get; private set; }
    public StatusPauta Status { get; private set; }
    public List<string> OpcoesDisponiveis { get; private set; } = new() { "Sim", "Não", "Abstenção" };
    public IReadOnlyCollection<VotoAssembleia> Votos => _votos.AsReadOnly();

    private PautaAssembleia() { }

    public static PautaAssembleia Create(
        Guid assembleiaId,
        string titulo,
        TipoVotacao tipoVotacao,
        int ordem = 1,
        string? descricao = null,
        List<string>? opcoesDisponiveis = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new AssembleiaDomainException("O título da pauta não pode ser vazio.");

        if (titulo.Length > 200)
            throw new AssembleiaDomainException("O título da pauta deve ter no máximo 200 caracteres.");

        var opcoes = opcoesDisponiveis != null && opcoesDisponiveis.Count > 0
            ? opcoesDisponiveis.Select(o => o.Trim()).ToList()
            : new List<string> { "Sim", "Não", "Abstenção" };

        return new PautaAssembleia
        {
            Id = Guid.NewGuid(),
            AssembleiaId = assembleiaId,
            Titulo = titulo.Trim(),
            Descricao = descricao?.Trim(),
            Ordem = ordem,
            TipoVotacao = tipoVotacao,
            Status = StatusPauta.Aberta,
            OpcoesDisponiveis = opcoes
        };
    }

    public VotoAssembleia RegistrarVoto(
        int tenantId,
        int condoId,
        string moradorUserId,
        string unidadeId,
        string opcaoEscolhida,
        double pesoVoto = 1.0)
    {
        if (Status != StatusPauta.Aberta)
            throw new AssembleiaDomainException($"A pauta '{Titulo}' já está encerrada para votação.");

        var unidadeNormalizada = unidadeId.Trim();

        // Invariante de Negócio: Voto único por unidade habitacional
        if (_votos.Any(v => v.UnidadeId.Equals(unidadeNormalizada, StringComparison.OrdinalIgnoreCase)))
        {
            throw new VotoDuplicadoException(unidadeNormalizada, Titulo);
        }

        var opcaoNormalizada = opcaoEscolhida.Trim();
        if (!OpcoesDisponiveis.Any(o => o.Equals(opcaoNormalizada, StringComparison.OrdinalIgnoreCase)))
        {
            throw new AssembleiaDomainException($"Opção de voto '{opcaoEscolhida}' inválida para a pauta '{Titulo}'. Opções válidas: {string.Join(", ", OpcoesDisponiveis)}.");
        }

        var voto = VotoAssembleia.Create(tenantId, condoId, AssembleiaId, Id, moradorUserId, unidadeNormalizada, opcaoNormalizada, pesoVoto);
        _votos.Add(voto);

        return voto;
    }

    public void EncerrarPauta()
    {
        Status = StatusPauta.Encerrada;
    }

    public Dictionary<string, int> ApurarContagemVotos()
    {
        var resultado = OpcoesDisponiveis.ToDictionary(op => op, _ => 0, StringComparer.OrdinalIgnoreCase);

        foreach (var voto in _votos)
        {
            var chave = resultado.Keys.FirstOrDefault(k => k.Equals(voto.OpcaoEscolhida, StringComparison.OrdinalIgnoreCase))
                ?? voto.OpcaoEscolhida;

            if (resultado.ContainsKey(chave))
                resultado[chave]++;
            else
                resultado[chave] = 1;
        }

        return resultado;
    }
}
