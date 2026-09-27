using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Domain.Visits;

public sealed class Visit : AggregateRoot<VisitId>
{
    private Visit(VisitId id, ShortUrlId shortUrlId, DateTimeOffset visitedAtUtc)
        : base(id)
    {
        ShortUrlId = shortUrlId;
        VisitedAtUtc = visitedAtUtc;
    }

    public ShortUrlId ShortUrlId { get; private set; }

    public DateTimeOffset VisitedAtUtc { get; private set; }

    internal static Visit Record(ShortUrlId shortUrlId, DateTimeOffset visitedAtUtc) =>
        new(VisitId.New(), shortUrlId, visitedAtUtc);
}
