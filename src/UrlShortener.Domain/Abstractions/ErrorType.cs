namespace UrlShortener.Domain.Abstractions;

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Gone
}
