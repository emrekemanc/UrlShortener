using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.ShortUrls;

namespace UrlShortener.Application.UnitTests.Fakes;

internal sealed class StubShortUrlReader : IShortUrlReader
{
    public IReadOnlyList<ShortUrlWithVisits> ShortUrls { get; init; } = [];

    public Task<PagedResponse<ShortUrlWithVisits>> ListAsync(PageRequest page, CancellationToken cancellationToken = default) =>
        Task.FromResult(ShortUrls.ToPage(page));
}
