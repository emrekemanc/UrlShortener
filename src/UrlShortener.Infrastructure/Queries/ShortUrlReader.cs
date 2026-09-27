using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.ShortUrls;
using UrlShortener.Application.Visits;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.Queries;

internal sealed class ShortUrlReader(ApplicationDbContext dbContext) : IShortUrlReader
{
    public async Task<PagedResponse<ShortUrlWithVisits>> ListAsync(
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await dbContext.ShortUrls.LongCountAsync(cancellationToken);

        var items = await dbContext.ShortUrls
            .AsNoTracking()
            .OrderByDescending(shortUrl => shortUrl.CreatedAtUtc)
            .ThenByDescending(shortUrl => shortUrl.Id)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(shortUrl => new ShortUrlWithVisits(
                shortUrl,
                new VisitStatistics(
                    dbContext.Visits.LongCount(visit => visit.ShortUrlId == shortUrl.Id),
                    dbContext.Visits
                        .Where(visit => visit.ShortUrlId == shortUrl.Id)
                        .Max(visit => (DateTimeOffset?)visit.VisitedAtUtc))))
            .ToListAsync(cancellationToken);

        return new PagedResponse<ShortUrlWithVisits>(items, page.Page, page.PageSize, totalCount);
    }
}
