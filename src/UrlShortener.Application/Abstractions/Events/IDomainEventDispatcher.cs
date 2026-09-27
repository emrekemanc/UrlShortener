using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Application.Abstractions.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
