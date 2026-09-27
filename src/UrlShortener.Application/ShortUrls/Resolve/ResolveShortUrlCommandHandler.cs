using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Domain.Visits;

namespace UrlShortener.Application.ShortUrls.Resolve;

internal sealed class ResolveShortUrlCommandHandler(
    IShortUrlRepository shortUrlRepository,
    IVisitRepository visitRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ResolveShortUrlCommand, string>
{
    public async Task<Result<string>> Handle(ResolveShortUrlCommand command, CancellationToken cancellationToken)
    {
        var shortUrlResult = await shortUrlRepository.FindByCodeAsync(command.Code, cancellationToken);
        if (shortUrlResult.IsFailure)
        {
            return shortUrlResult.Error;
        }

        var shortUrl = shortUrlResult.Value;

        var visitResult = shortUrl.RecordVisit(timeProvider.GetUtcNow());
        if (visitResult.IsFailure)
        {
            return visitResult.Error;
        }

        visitRepository.Add(visitResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return shortUrl.OriginalUrl.Value;
    }
}
