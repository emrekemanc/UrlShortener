using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls;

public sealed record OriginalUrl
{
    public const int MaxLength = 2048;

    private OriginalUrl(string value) => Value = value;

    public string Value { get; }

    public static Result<OriginalUrl> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ShortUrlErrors.EmptyUrl;
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            return ShortUrlErrors.UrlTooLong;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        {
            return ShortUrlErrors.InvalidUrl;
        }

        return new OriginalUrl(value);
    }
}
