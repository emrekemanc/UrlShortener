namespace UrlShortener.Application.Abstractions.Data;

public sealed class UniqueConstraintViolationException(Exception innerException)
    : Exception("A unique constraint was violated while saving changes.", innerException);
