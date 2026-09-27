using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls;

/// <summary>
/// The unique, URL-safe key that identifies a short URL (e.g. "aZ3k9Qx" in https://sho.rt/aZ3k9Qx).
/// </summary>
public sealed record ShortCode
{
    public const int MinLength = 4;
    public const int MaxLength = 32;
    public const int GeneratedLength = 7;

    // Codes that would collide with the application's own routes.
    private static readonly HashSet<string> ReservedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "health",
        "openapi",
        "swagger"
    };

    private ShortCode(string value) => Value = value;

    public string Value { get; }

    public static Result<ShortCode> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ShortUrlErrors.EmptyCode;
        }

        value = value.Trim();

        if (value.Length is < MinLength or > MaxLength)
        {
            return ShortUrlErrors.InvalidCodeLength;
        }

        if (!value.All(IsAllowedCharacter))
        {
            return ShortUrlErrors.InvalidCodeCharacters;
        }

        if (ReservedCodes.Contains(value))
        {
            return ShortUrlErrors.ReservedCode;
        }

        return new ShortCode(value);
    }

    public override string ToString() => Value;

    private static bool IsAllowedCharacter(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_';
}
