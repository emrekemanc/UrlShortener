using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Domain.ShortUrls;

public static class ShortUrlErrors
{
    public static readonly Error EmptyUrl = Error.Validation(
        "ShortUrl.EmptyUrl",
        "URL is required.");

    public static readonly Error UrlTooLong = Error.Validation(
        "ShortUrl.UrlTooLong",
        $"URL cannot be longer than {OriginalUrl.MaxLength} characters.");

    public static readonly Error InvalidUrl = Error.Validation(
        "ShortUrl.InvalidUrl",
        "URL must be an absolute http or https address.");

    public static readonly Error EmptyCode = Error.Validation(
        "ShortUrl.EmptyCode",
        "Short code is required.");

    public static readonly Error InvalidCodeLength = Error.Validation(
        "ShortUrl.InvalidCodeLength",
        $"Short code must be between {ShortCode.MinLength} and {ShortCode.MaxLength} characters.");

    public static readonly Error InvalidCodeCharacters = Error.Validation(
        "ShortUrl.InvalidCodeCharacters",
        "Short code may only contain letters, digits, '-' and '_'.");

    public static readonly Error ReservedCode = Error.Validation(
        "ShortUrl.ReservedCode",
        "This short code is reserved.");

    public static readonly Error ExpirationInPast = Error.Validation(
        "ShortUrl.ExpirationInPast",
        "Expiration date must be in the future.");

    public static readonly Error Expired = Error.Gone(
        "ShortUrl.Expired",
        "This short URL has expired.");

    public static readonly Error Deactivated = Error.Gone(
        "ShortUrl.Deactivated",
        "This short URL has been deactivated.");

    public static readonly Error AlreadyDeactivated = Error.Conflict(
        "ShortUrl.AlreadyDeactivated",
        "This short URL is already deactivated.");

    public static readonly Error CodeGenerationFailed = Error.Failure(
        "ShortUrl.CodeGenerationFailed",
        "A unique short code could not be generated. Please try again.");

    public static Error NotFound(string? code) => Error.NotFound(
        "ShortUrl.NotFound",
        $"Short URL '{code}' was not found.");

    public static Error CodeAlreadyTaken(ShortCode code) => Error.Conflict(
        "ShortUrl.CodeAlreadyTaken",
        $"Short code '{code.Value}' is already taken.");
}
