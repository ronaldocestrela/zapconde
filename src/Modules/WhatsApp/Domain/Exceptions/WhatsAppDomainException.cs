namespace Modules.WhatsApp.Domain.Exceptions;

/// <summary>
/// Exceção de domínio lançada para violações de invariantes no módulo de WhatsApp.
/// </summary>
public class WhatsAppDomainException : Exception
{
    public WhatsAppDomainException(string message) : base(message)
    {
    }

    public WhatsAppDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
