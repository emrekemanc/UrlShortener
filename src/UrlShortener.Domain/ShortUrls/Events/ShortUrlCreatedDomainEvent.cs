using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls.Events;

public sealed record ShortUrlCreatedDomainEvent(
    ShortUrlId ShortUrlId,
    string Code,
    string OriginalUrl,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
