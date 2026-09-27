using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Application.ShortUrls.GetAll;

internal sealed class GetShortUrlsQueryHandler(IShortUrlReader reader, TimeProvider timeProvider)
    : IQueryHandler<GetShortUrlsQuery, PagedResponse<ShortUrlResponse>>
{
    public async Task<Result<PagedResponse<ShortUrlResponse>>> Handle(
        GetShortUrlsQuery query,
        CancellationToken cancellationToken)
    {
        var pageResult = PageRequest.Create(query.Page, query.PageSize);
        if (pageResult.IsFailure)
        {
            return pageResult.Error;
        }

        var shortUrls = await reader.ListAsync(pageResult.Value, cancellationToken);
        var utcNow = timeProvider.GetUtcNow();

        return shortUrls.Map(item => item.ShortUrl.ToResponse(item.Visits, utcNow));
    }
}
