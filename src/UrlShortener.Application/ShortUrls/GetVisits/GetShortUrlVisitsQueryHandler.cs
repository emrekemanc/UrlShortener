using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls.GetVisits;

internal sealed class GetShortUrlVisitsQueryHandler(IShortUrlRepository repository, IVisitReader visitReader)
    : IQueryHandler<GetShortUrlVisitsQuery, PagedResponse<VisitResponse>>
{
    public async Task<Result<PagedResponse<VisitResponse>>> Handle(
        GetShortUrlVisitsQuery query,
        CancellationToken cancellationToken)
    {
        var pageResult = PageRequest.Create(query.Page, query.PageSize);
        if (pageResult.IsFailure)
        {
            return pageResult.Error;
        }

        var shortUrlResult = await repository.FindByCodeAsync(query.Code, cancellationToken);
        if (shortUrlResult.IsFailure)
        {
            return shortUrlResult.Error;
        }

        return await visitReader.ListAsync(shortUrlResult.Value.Id, pageResult.Value, cancellationToken);
    }
}
