using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Domain.Exceptions;

public class InvalidOcorrenciaStatusTransitionException : Exception
{
    public StatusOcorrencia StatusAtual { get; }
    public StatusOcorrencia StatusNovo { get; }

    public InvalidOcorrenciaStatusTransitionException(StatusOcorrencia statusAtual, StatusOcorrencia statusNovo)
        : base($"Transicao invalida do status '{statusAtual}' para '{statusNovo}'.")
    {
        StatusAtual = statusAtual;
        StatusNovo = statusNovo;
    }
}
