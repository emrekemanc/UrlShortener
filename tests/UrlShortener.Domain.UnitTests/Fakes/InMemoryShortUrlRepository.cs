using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Domain.UnitTests.Fakes;

internal sealed class InMemoryShortUrlRepository : IShortUrlRepository
{
    private readonly List<ShortUrl> _shortUrls = [];

    public Task<ShortUrl?> GetByCodeAsync(ShortCode code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_shortUrls.SingleOrDefault(shortUrl => shortUrl.Code == code));

    public Task<bool> ExistsAsync(ShortCode code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_shortUrls.Any(shortUrl => shortUrl.Code == code));

    public void Add(ShortUrl shortUrl) => _shortUrls.Add(shortUrl);
}
