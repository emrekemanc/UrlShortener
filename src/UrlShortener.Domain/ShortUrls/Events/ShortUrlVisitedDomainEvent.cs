using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls.Events;

public sealed record ShortUrlVisitedDomainEvent(
    ShortUrlId ShortUrlId,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
