using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls.Create;

internal sealed class CreateShortUrlCommandHandler(
    IShortUrlRepository repository,
    IShortCodeGenerator codeGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CreateShortUrlCommand, ShortUrlResponse>
{
    internal const int MaxGenerationAttempts = 5;

    public async Task<Result<ShortUrlResponse>> Handle(
        CreateShortUrlCommand command,
        CancellationToken cancellationToken)
    {
        var originalUrlResult = OriginalUrl.Create(command.Url);
        if (originalUrlResult.IsFailure)
        {
            return originalUrlResult.Error;
        }

        var codeResult = string.IsNullOrWhiteSpace(command.CustomCode)
            ? await GenerateUniqueCodeAsync(cancellationToken)
            : await EnsureCustomCodeIsAvailableAsync(command.CustomCode, cancellationToken);
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
            // Another request claimed the same code between our check and the insert.
            return ShortUrlErrors.CodeAlreadyTaken(shortUrl.Code);
        }

        return shortUrl.ToResponse(utcNow);
    }

    private async Task<Result<ShortCode>> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            var code = codeGenerator.Generate();

            if (!await repository.ExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        return ShortUrlErrors.CodeGenerationFailed;
    }

    private async Task<Result<ShortCode>> EnsureCustomCodeIsAvailableAsync(
        string customCode,
        CancellationToken cancellationToken)
    {
        var codeResult = ShortCode.Create(customCode);
        if (codeResult.IsFailure)
        {
            return codeResult;
        }

        return await repository.ExistsAsync(codeResult.Value, cancellationToken)
            ? ShortUrlErrors.CodeAlreadyTaken(codeResult.Value)
            : codeResult;
    }
}
