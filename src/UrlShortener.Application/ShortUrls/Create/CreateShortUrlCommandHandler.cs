using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls.Create;

internal sealed class CreateShortUrlCommandHandler(
    IShortUrlRepository repository,
    ShortCodeAllocator codeAllocator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CreateShortUrlCommand, ShortUrlResponse>
{
    public async Task<Result<ShortUrlResponse>> Handle(
        CreateShortUrlCommand command,
        CancellationToken cancellationToken)
    {
        var originalUrlResult = OriginalUrl.Create(command.Url);
        if (originalUrlResult.IsFailure)
        {
            return originalUrlResult.Error;
        }

        var codeResult = await codeAllocator.AllocateAsync(command.CustomCode, cancellationToken);
        if (codeResult.IsFailure)
        {
            return codeResult.Error;
        }

        var utcNow = timeProvider.GetUtcNow();

        var shortUrlResult = ShortUrl.Create(codeResult.Value, originalUrlResult.Value, command.ExpiresAtUtc, utcNow);
        if (shortUrlResult.IsFailure)
        {
            return shortUrlResult.Error;
        }

        var shortUrl = shortUrlResult.Value;
        repository.Add(shortUrl);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            return ShortUrlErrors.CodeAlreadyTaken(shortUrl.Code);
        }

        return shortUrl.ToResponse(VisitStatistics.None, utcNow);
    }
}
