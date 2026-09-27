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
