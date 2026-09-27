using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls.Events;

public sealed record ShortUrlDeactivatedDomainEvent(
    ShortUrlId ShortUrlId,
    string Code,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
