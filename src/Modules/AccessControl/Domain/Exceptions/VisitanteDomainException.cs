namespace Modules.AccessControl.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre violação de invariante de domínio na gestão de visitantes.
/// </summary>
public class VisitanteDomainException : Exception
{
    public VisitanteDomainException(string message) : base(message) { }
}
