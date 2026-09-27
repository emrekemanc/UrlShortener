using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.ShortUrls.Create;

public sealed record CreateShortUrlCommand(
    string? Url,
    string? CustomCode,
    DateTimeOffset? ExpiresAtUtc) : ICommand<ShortUrlResponse>;
