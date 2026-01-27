namespace MYP.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
