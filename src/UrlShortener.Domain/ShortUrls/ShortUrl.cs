using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls.Events;
using UrlShortener.Domain.Visits;

namespace UrlShortener.Domain.ShortUrls;

public sealed class ShortUrl : AggregateRoot<ShortUrlId>
{
    private ShortUrl(
        ShortUrlId id,
        ShortCode code,
        OriginalUrl originalUrl,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc)
        : base(id)
    {
        Code = code;
        OriginalUrl = originalUrl;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public ShortCode Code { get; private set; }

    public OriginalUrl OriginalUrl { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public DateTimeOffset? DeactivatedAtUtc { get; private set; }

    public static Result<ShortUrl> Create(
        ShortCode code,
        OriginalUrl originalUrl,
        DateTimeOffset? expiresAtUtc,
        DateTimeOffset utcNow)
    {
        if (expiresAtUtc <= utcNow)
        {
            return ShortUrlErrors.ExpirationInPast;
        }

        var shortUrl = new ShortUrl(ShortUrlId.New(), code, originalUrl, utcNow, expiresAtUtc?.ToUniversalTime());
        shortUrl.Raise(new ShortUrlCreatedDomainEvent(shortUrl.Id, code.Value, originalUrl.Value, utcNow));

        return shortUrl;
    }

    public ShortUrlStatus GetStatus(DateTimeOffset utcNow) =>
        DeactivatedAtUtc is not null ? ShortUrlStatus.Deactivated :
        ExpiresAtUtc <= utcNow ? ShortUrlStatus.Expired :
        ShortUrlStatus.Active;

    public Result<Visit> RecordVisit(DateTimeOffset utcNow) => GetStatus(utcNow) switch
    {
        ShortUrlStatus.Deactivated => ShortUrlErrors.Deactivated,
        ShortUrlStatus.Expired => ShortUrlErrors.Expired,
        _ => Visit.Record(Id, utcNow)
    };

    public Result Deactivate(DateTimeOffset utcNow)
    {
        if (DeactivatedAtUtc is not null)
        {
            return Result.Failure(ShortUrlErrors.AlreadyDeactivated);
        }

        DeactivatedAtUtc = utcNow;
        Raise(new ShortUrlDeactivatedDomainEvent(Id, Code.Value, utcNow));

        return Result.Success();
    }
}
