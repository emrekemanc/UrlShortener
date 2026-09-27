using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls;

public sealed record ShortUrlResponse(
    Guid Id,
    string Code,
    string OriginalUrl,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc,
    DateTimeOffset? DeactivatedAtUtc,
    long VisitCount,
    DateTimeOffset? LastVisitedAtUtc);

internal static class ShortUrlMappings
{
    public static ShortUrlResponse ToResponse(this ShortUrl shortUrl, DateTimeOffset utcNow) => new(
        shortUrl.Id.Value,
        shortUrl.Code.Value,
        shortUrl.OriginalUrl.Value,
        shortUrl.GetStatus(utcNow).ToString(),
        shortUrl.CreatedAtUtc,
        shortUrl.ExpiresAtUtc,
        shortUrl.DeactivatedAtUtc,
        shortUrl.VisitCount,
        shortUrl.LastVisitedAtUtc);
}
