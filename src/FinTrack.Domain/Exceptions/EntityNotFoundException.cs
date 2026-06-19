namespace FinTrack.Domain.Exceptions;

/// <summary>
/// Thrown when a lookup for a specific entity by id returns nothing.
/// Used in Application handlers when, for example, a transaction id
/// or category id from a request does not exist in the database.
/// </summary>
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" with key ({key}) was not found.") { }
}