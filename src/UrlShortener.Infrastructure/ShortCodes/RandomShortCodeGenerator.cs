using System.Security.Cryptography;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Infrastructure.ShortCodes;

/// <summary>
/// Generates cryptographically random Base62 codes. 62^7 ≈ 3.5 trillion combinations.
/// </summary>
internal sealed class RandomShortCodeGenerator : IShortCodeGenerator
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public ShortCode Generate()
    {
        while (true)
        {
            var candidate = RandomNumberGenerator.GetString(Alphabet, ShortCode.GeneratedLength);

            // Skips the (astronomically rare) case of hitting a reserved word.
            var result = ShortCode.Create(candidate);
            if (result.IsSuccess)
            {
                return result.Value;
            }
        }
    }
}
