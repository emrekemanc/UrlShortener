namespace UrlShortener.Domain.ShortUrls;

public interface IShortUrlRepository
{
    Task<ShortUrl?> GetByCodeAsync(ShortCode code, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(ShortCode code, CancellationToken cancellationToken = default);

    void Add(ShortUrl shortUrl);
}
