using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.ShortUrls;

internal static class TestData
{
    public const string Url = "https://example.com/some/long/path";

    public static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static ShortUrl CreateShortUrl(string code, DateTimeOffset? expiresAtUtc = null)
    {
        var shortUrl = ShortUrl.Create(
            ShortCode.Create(code).Value,
            OriginalUrl.Create(Url).Value,
            expiresAtUtc,
            Now).Value;

        shortUrl.ClearDomainEvents();
        return shortUrl;
    }
}
