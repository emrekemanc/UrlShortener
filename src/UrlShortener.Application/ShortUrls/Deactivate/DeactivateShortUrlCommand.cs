using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.ShortUrls.Deactivate;

public sealed record DeactivateShortUrlCommand(string Code) : ICommand;
