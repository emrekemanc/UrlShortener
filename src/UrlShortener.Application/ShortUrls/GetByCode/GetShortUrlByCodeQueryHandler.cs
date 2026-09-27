using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls.GetByCode;

internal sealed class GetShortUrlByCodeQueryHandler(
    IShortUrlRepository repository,
    TimeProvider timeProvider) : IQueryHandler<GetShortUrlByCodeQuery, ShortUrlResponse>
{
    public async Task<Result<ShortUrlResponse>> Handle(GetShortUrlByCodeQuery query, CancellationToken cancellationToken)
    {
        var shortUrlResult = await repository.FindByCodeAsync(query.Code, cancellationToken);
        if (shortUrlResult.IsFailure)
        {
            return shortUrlResult.Error;
        }

        return shortUrlResult.Value.ToResponse(timeProvider.GetUtcNow());
    }
}
