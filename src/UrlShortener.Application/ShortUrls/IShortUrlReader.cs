using UrlShortener.Application.Abstractions.Paging;

namespace UrlShortener.Application.ShortUrls;

public interface IShortUrlReader
{
    Task<PagedResponse<ShortUrlWithVisits>> ListAsync(PageRequest page, CancellationToken cancellationToken = default);
}
