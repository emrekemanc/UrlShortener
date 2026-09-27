using UrlShortener.Application.Visits;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls;

public sealed record ShortUrlWithVisits(ShortUrl ShortUrl, VisitStatistics Visits);
