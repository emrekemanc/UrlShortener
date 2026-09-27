using System.Security.Cryptography;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Infrastructure.ShortCodes;

internal sealed class RandomShortCodeGenerator : IShortCodeGenerator
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public ShortCode Generate()
    {
        while (true)
        {
            var result = ShortCode.Create(RandomNumberGenerator.GetString(Alphabet, ShortCode.GeneratedLength));
            if (result.IsSuccess)
            {
                return result.Value;
            }
        }
    }
}
