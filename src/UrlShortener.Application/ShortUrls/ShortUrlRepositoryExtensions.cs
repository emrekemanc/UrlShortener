using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls;

internal static class ShortUrlRepositoryExtensions
{
    /// <summary>
    /// Loads a short URL by its raw code. A code that is not even valid cannot exist,
    /// so it is reported as "not found" rather than as a validation error.
    /// </summary>
    public static async Task<Result<ShortUrl>> FindByCodeAsync(
        this IShortUrlRepository repository,
        string? code,
        CancellationToken cancellationToken)
    {
        var codeResult = ShortCode.Create(code);
        if (codeResult.IsFailure)
        {
            return ShortUrlErrors.NotFound(code);
        }

        var shortUrl = await repository.GetByCodeAsync(codeResult.Value, cancellationToken);
        if (shortUrl is null)
        {
            return ShortUrlErrors.NotFound(code);
        }

        return shortUrl;
    }
}
