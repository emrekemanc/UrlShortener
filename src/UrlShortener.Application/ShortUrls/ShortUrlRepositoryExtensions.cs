using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.ShortUrls;

internal static class ShortUrlRepositoryExtensions
{
    public static async Task<Result<ShortUrl>> FindByCodeAsync(
        this IShortUrlRepository repository,
        string? code,
        CancellationToken cancellationToken)
    {
        var codeResult = ShortCode.Create(code);
        var shortUrl = codeResult.IsSuccess
            ? await repository.GetByCodeAsync(codeResult.Value, cancellationToken)
            : null;

        return shortUrl is null ? ShortUrlErrors.NotFound(code) : shortUrl;
    }
}
