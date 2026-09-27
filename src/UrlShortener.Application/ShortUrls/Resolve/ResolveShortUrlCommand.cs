using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.ShortUrls.Resolve;

public sealed record ResolveShortUrlCommand(string Code) : ICommand<string>;
