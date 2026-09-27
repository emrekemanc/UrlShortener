using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Events;
using UrlShortener.Domain.ShortUrls.Events;

namespace UrlShortener.Application.ShortUrls.EventHandlers;

internal sealed class ShortUrlCreatedLoggingHandler(ILogger<ShortUrlCreatedLoggingHandler> logger)
    : IDomainEventHandler<ShortUrlCreatedDomainEvent>
{
    public Task Handle(ShortUrlCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Short URL {Code} created for {OriginalUrl}",
            domainEvent.Code,
            domainEvent.OriginalUrl);

        return Task.CompletedTask;
    }
}
