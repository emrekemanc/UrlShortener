namespace UrlShortener.Api.Contracts;

public sealed record CreateShortUrlRequest(
    string Url,
    string? CustomCode,
    DateTimeOffset? ExpiresAtUtc);
