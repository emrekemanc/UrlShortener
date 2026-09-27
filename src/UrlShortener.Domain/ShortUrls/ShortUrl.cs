using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls.Events;

namespace UrlShortener.Domain.ShortUrls;

/// <summary>
/// Aggregate root: a short code that redirects to an original URL, with its lifecycle
/// (expiration, deactivation) and visit statistics.
/// </summary>
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

    // Required by EF Core for materialization.
    private ShortUrl()
    {
    }

    public ShortCode Code { get; private set; } = null!;

    public OriginalUrl OriginalUrl { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public DateTimeOffset? DeactivatedAtUtc { get; private set; }

    public long VisitCount { get; private set; }

    public DateTimeOffset? LastVisitedAtUtc { get; private set; }

    public static Result<ShortUrl> Create(
        ShortCode code,
        OriginalUrl originalUrl,
        DateTimeOffset? expiresAtUtc,
        DateTimeOffset utcNow)
    {
        if (expiresAtUtc is { } expiresAt && expiresAt <= utcNow)
        {
            return ShortUrlErrors.ExpirationInPast;
        }

        var shortUrl = new ShortUrl(
            ShortUrlId.New(),
            code,
            originalUrl,
            utcNow,
            expiresAtUtc?.ToUniversalTime());

        shortUrl.Raise(new ShortUrlCreatedDomainEvent(shortUrl.Id, code.Value, originalUrl.Value, utcNow));

        return shortUrl;
    }

    public ShortUrlStatus GetStatus(DateTimeOffset utcNow)
    {
        if (DeactivatedAtUtc is not null)
        {
            return ShortUrlStatus.Deactivated;
        }

        return ExpiresAtUtc is { } expiresAt && expiresAt <= utcNow
            ? ShortUrlStatus.Expired
            : ShortUrlStatus.Active;
    }

    /// <summary>
    /// Records a visit and returns the address to redirect to, if the short URL is still usable.
    /// </summary>
    public Result<OriginalUrl> Visit(DateTimeOffset utcNow)
    {
        var status = GetStatus(utcNow);

        if (status == ShortUrlStatus.Deactivated)
        {
            return ShortUrlErrors.Deactivated;
        }

        if (status == ShortUrlStatus.Expired)
        {
            return ShortUrlErrors.Expired;
        }

        VisitCount++;
        LastVisitedAtUtc = utcNow;

        Raise(new ShortUrlVisitedDomainEvent(Id, utcNow));

        return OriginalUrl;
    }

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
