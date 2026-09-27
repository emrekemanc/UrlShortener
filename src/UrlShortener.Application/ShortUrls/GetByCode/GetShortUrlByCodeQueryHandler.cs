using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls.GetByCode;

internal sealed class GetShortUrlByCodeQueryHandler(
    IShortUrlRepository repository,
    IVisitReader visitReader,
    TimeProvider timeProvider) : IQueryHandler<GetShortUrlByCodeQuery, ShortUrlResponse>
{
    public async Task<Result<ShortUrlResponse>> Handle(GetShortUrlByCodeQuery query, CancellationToken cancellationToken)
    {
        var shortUrlResult = await repository.FindByCodeAsync(query.Code, cancellationToken);
        if (shortUrlResult.IsFailure)
        {
            return shortUrlResult.Error;
        }

        var shortUrl = shortUrlResult.Value;
        var visits = await visitReader.GetStatisticsAsync(shortUrl.Id, cancellationToken);

        return shortUrl.ToResponse(visits, timeProvider.GetUtcNow());
    }
}
