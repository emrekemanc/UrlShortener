using UrlShortener.Application.Visits;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls;

internal static class ShortUrlMappings
{
    public static ShortUrlResponse ToResponse(this ShortUrl shortUrl, VisitStatistics visits, DateTimeOffset utcNow) => new(
        shortUrl.Id.Value,
        shortUrl.Code.Value,
        shortUrl.OriginalUrl.Value,
        shortUrl.GetStatus(utcNow).ToString(),
        shortUrl.CreatedAtUtc,
        shortUrl.ExpiresAtUtc,
        shortUrl.DeactivatedAtUtc,
        visits.TotalVisits,
        visits.LastVisitedAtUtc);
}
