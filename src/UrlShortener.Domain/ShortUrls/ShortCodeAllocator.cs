using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls;

public sealed class ShortCodeAllocator(IShortUrlRepository repository, IShortCodeGenerator generator)
{
    public const int MaxGenerationAttempts = 5;

    public Task<Result<ShortCode>> AllocateAsync(string? requestedCode, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(requestedCode)
            ? GenerateAsync(cancellationToken)
            : ClaimAsync(requestedCode, cancellationToken);

    private async Task<Result<ShortCode>> GenerateAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            var code = generator.Generate();

            if (!await repository.ExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        return ShortUrlErrors.CodeGenerationFailed;
    }

    private async Task<Result<ShortCode>> ClaimAsync(string requestedCode, CancellationToken cancellationToken)
    {
        var codeResult = ShortCode.Create(requestedCode);
        if (codeResult.IsFailure)
        {
            return codeResult;
        }

        return await repository.ExistsAsync(codeResult.Value, cancellationToken)
            ? ShortUrlErrors.CodeAlreadyTaken(codeResult.Value)
            : codeResult;
    }
}
