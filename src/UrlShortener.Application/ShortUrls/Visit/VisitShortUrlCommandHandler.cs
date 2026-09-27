using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls.Visit;

internal sealed class VisitShortUrlCommandHandler(
    IShortUrlRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<VisitShortUrlCommand, string>
{
    public async Task<Result<string>> Handle(VisitShortUrlCommand command, CancellationToken cancellationToken)
    {
        var shortUrlResult = await repository.FindByCodeAsync(command.Code, cancellationToken);
        if (shortUrlResult.IsFailure)
        {
            return shortUrlResult.Error;
        }

        var visitResult = shortUrlResult.Value.Visit(timeProvider.GetUtcNow());
        if (visitResult.IsFailure)
        {
            return visitResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return visitResult.Value.Value;
    }
}
