using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.ShortUrls.Visit;

/// <summary>
/// Resolves a short code for redirection and records the visit. Returns the original URL.
/// </summary>
public sealed record VisitShortUrlCommand(string Code) : ICommand<string>;
