using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.Repositories;

internal sealed class ShortUrlRepository(ApplicationDbContext dbContext) : IShortUrlRepository
{
    public Task<ShortUrl?> GetByCodeAsync(ShortCode code, CancellationToken cancellationToken = default) =>
        dbContext.ShortUrls.SingleOrDefaultAsync(shortUrl => shortUrl.Code == code, cancellationToken);

    public Task<bool> ExistsAsync(ShortCode code, CancellationToken cancellationToken = default) =>
        dbContext.ShortUrls.AnyAsync(shortUrl => shortUrl.Code == code, cancellationToken);

    public void Add(ShortUrl shortUrl) => dbContext.ShortUrls.Add(shortUrl);
}
