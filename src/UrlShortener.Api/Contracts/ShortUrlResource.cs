using UrlShortener.Application.ShortUrls;

namespace UrlShortener.Api.Contracts;

public sealed record ShortUrlResource(
    Guid Id,
    string Code,
    string ShortLink,
    string OriginalUrl,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc,
    DateTimeOffset? DeactivatedAtUtc,
    long VisitCount,
    DateTimeOffset? LastVisitedAtUtc)
{
    public static ShortUrlResource From(ShortUrlResponse response, HttpRequest request) => new(
        response.Id,
        response.Code,
        $"{request.Scheme}://{request.Host}{request.PathBase}/{response.Code}",
        response.OriginalUrl,
        response.Status,
        response.CreatedAtUtc,
        response.ExpiresAtUtc,
        response.DeactivatedAtUtc,
        response.VisitCount,
        response.LastVisitedAtUtc);
}
