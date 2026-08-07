namespace Modules.AIEngine.Domain.Exceptions;

/// <summary>
/// Exceção de domínio para regras de negócio e invariantes do módulo AIEngine
/// </summary>
public class AiEngineDomainException : Exception
{
    public AiEngineDomainException(string message) : base(message)
    {
    }

    public AiEngineDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
