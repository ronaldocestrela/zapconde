namespace Modules.Operations.Domain.Exceptions;

public class AssembleiaDomainException : Exception
{
    public AssembleiaDomainException(string message) : base(message)
    {
    }
}

public class VotoDuplicadoException : AssembleiaDomainException
{
    public VotoDuplicadoException(string unidadeId, string pautaTitulo)
        : base($"A unidade habitacional '{unidadeId}' já registrou voto na pauta '{pautaTitulo}'. Cada unidade pode votar apenas uma vez por pauta.")
    {
    }
}

public class AssembleiaEncerradaException : AssembleiaDomainException
{
    public AssembleiaEncerradaException(string assembleiaTitulo)
        : base($"A assembleia '{assembleiaTitulo}' não está aberta para votações no momento.")
    {
    }
}
