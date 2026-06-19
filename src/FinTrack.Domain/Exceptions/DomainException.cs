namespace FinTrack.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated — i.e. when code attempts
/// to put an entity into a state that should be impossible according to
/// the business rules of the domain itself. This is distinct from
/// validation errors (bad user input) which are handled in the
/// Application layer via FluentValidation.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}