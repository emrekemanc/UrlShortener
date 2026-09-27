using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Events;
using UrlShortener.Domain.ShortUrls.Events;

namespace UrlShortener.Application.ShortUrls.EventHandlers;

internal sealed class ShortUrlDeactivatedLoggingHandler(ILogger<ShortUrlDeactivatedLoggingHandler> logger)
    : IDomainEventHandler<ShortUrlDeactivatedDomainEvent>
{
    public Task Handle(ShortUrlDeactivatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Short URL {Code} deactivated", domainEvent.Code);

        return Task.CompletedTask;
    }
}
