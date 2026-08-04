namespace Modules.Identity.Domain;

public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
}
