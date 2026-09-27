using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Domain.Visits;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.Queries;

internal sealed class VisitReader(ApplicationDbContext dbContext) : IVisitReader
{
    public async Task<VisitStatistics> GetStatisticsAsync(ShortUrlId shortUrlId, CancellationToken cancellationToken = default)
    {
        var visits = VisitsOf(shortUrlId);

        var totalVisits = await visits.LongCountAsync(cancellationToken);
        if (totalVisits == 0)
        {
            return VisitStatistics.None;
        }

        var lastVisitedAtUtc = await visits.MaxAsync(visit => visit.VisitedAtUtc, cancellationToken);

        return new VisitStatistics(totalVisits, lastVisitedAtUtc);
    }

    public async Task<PagedResponse<VisitResponse>> ListAsync(
        ShortUrlId shortUrlId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var visits = VisitsOf(shortUrlId);

        var totalCount = await visits.LongCountAsync(cancellationToken);
        var items = await visits
            .OrderByDescending(visit => visit.VisitedAtUtc)
            .ThenByDescending(visit => visit.Id)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(visit => new VisitResponse(visit.Id.Value, visit.VisitedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResponse<VisitResponse>(items, page.Page, page.PageSize, totalCount);
    }

    private IQueryable<Visit> VisitsOf(ShortUrlId shortUrlId) =>
        dbContext.Visits.AsNoTracking().Where(visit => visit.ShortUrlId == shortUrlId);
}
