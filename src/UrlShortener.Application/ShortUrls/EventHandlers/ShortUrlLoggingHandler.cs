using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Events;
using UrlShortener.Domain.ShortUrls.Events;

namespace UrlShortener.Application.ShortUrls.EventHandlers;

internal sealed partial class ShortUrlLoggingHandler(ILogger<ShortUrlLoggingHandler> logger)
    : IDomainEventHandler<ShortUrlCreatedDomainEvent>,
      IDomainEventHandler<ShortUrlDeactivatedDomainEvent>
{
    public Task Handle(ShortUrlCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        LogCreated(logger, domainEvent.Code, domainEvent.OriginalUrl);
        return Task.CompletedTask;
    }

    public Task Handle(ShortUrlDeactivatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        LogDeactivated(logger, domainEvent.Code);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Short URL {Code} created for {OriginalUrl}")]
    private static partial void LogCreated(ILogger logger, string code, string originalUrl);

    [LoggerMessage(Level = LogLevel.Information, Message = "Short URL {Code} deactivated")]
    private static partial void LogDeactivated(ILogger logger, string code);
}
