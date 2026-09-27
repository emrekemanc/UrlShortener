namespace UrlShortener.Domain.ShortUrls;

public readonly record struct ShortUrlId(Guid Value)
{
    public static ShortUrlId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}
