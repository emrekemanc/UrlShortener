using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.Visits;

public interface IVisitReader
{
    Task<VisitStatistics> GetStatisticsAsync(ShortUrlId shortUrlId, CancellationToken cancellationToken = default);

    Task<PagedResponse<VisitResponse>> ListAsync(
        ShortUrlId shortUrlId,
        PageRequest page,
        CancellationToken cancellationToken = default);
}
