namespace UrlShortener.Application.Abstractions.Data;

/// <summary>
/// Thrown by the persistence layer when a save violates a unique constraint
/// (e.g. two requests racing for the same short code).
/// </summary>
public sealed class UniqueConstraintViolationException(Exception innerException)
    : Exception("A unique constraint was violated while saving changes.", innerException);
