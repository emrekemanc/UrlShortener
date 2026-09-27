using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls.Deactivate;

internal sealed class DeactivateShortUrlCommandHandler(
    IShortUrlRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<DeactivateShortUrlCommand>
{
    public async Task<Result> Handle(DeactivateShortUrlCommand command, CancellationToken cancellationToken)
    {
        var shortUrlResult = await repository.FindByCodeAsync(command.Code, cancellationToken);
        if (shortUrlResult.IsFailure)
        {
            return Result.Failure(shortUrlResult.Error);
        }

        var deactivateResult = shortUrlResult.Value.Deactivate(timeProvider.GetUtcNow());
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
