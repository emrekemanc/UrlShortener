using System.Collections.Frozen;
using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls;

public sealed record ShortCode
{
    public const int MinLength = 4;
    public const int MaxLength = 32;
    public const int GeneratedLength = 7;

    private static readonly FrozenSet<string> ReservedCodes =
        new[] { "admin", "health", "openapi", "swagger" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

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

    private static bool IsAllowedCharacter(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_';
}
