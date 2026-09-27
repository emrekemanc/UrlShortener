namespace UrlShortener.Domain.ShortUrls;

/// <summary>
/// Produces candidate short codes. Uniqueness is checked by the caller.
/// </summary>
public interface IShortCodeGenerator
{
    ShortCode Generate();
}
