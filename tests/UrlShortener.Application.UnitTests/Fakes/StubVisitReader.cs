using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.Fakes;

internal sealed class StubVisitReader : IVisitReader
{
    public VisitStatistics Statistics { get; init; } = VisitStatistics.None;

    public IReadOnlyList<VisitResponse> Visits { get; init; } = [];

    public Task<VisitStatistics> GetStatisticsAsync(ShortUrlId shortUrlId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Statistics);

    public Task<PagedResponse<VisitResponse>> ListAsync(
        ShortUrlId shortUrlId,
        PageRequest page,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Visits.ToPage(page));
}
