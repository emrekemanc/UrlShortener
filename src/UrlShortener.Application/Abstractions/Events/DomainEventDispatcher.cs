using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Application.Abstractions.Events;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, HandlerInvoker> Invokers = new();

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var invoker = Invokers.GetOrAdd(
                domainEvent.GetType(),
                static eventType => (HandlerInvoker)Activator.CreateInstance(
                    typeof(HandlerInvoker<>).MakeGenericType(eventType))!);

            await invoker.InvokeAsync(domainEvent, serviceProvider, cancellationToken);
        }
    }

    // Bridges the runtime event type to the strongly typed IDomainEventHandler<T> without reflection per call.
    private abstract class HandlerInvoker
    {
        public abstract Task InvokeAsync(
            IDomainEvent domainEvent,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    private sealed class HandlerInvoker<TDomainEvent> : HandlerInvoker
        where TDomainEvent : IDomainEvent
    {
        public override async Task InvokeAsync(
            IDomainEvent domainEvent,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>())
            {
                await handler.Handle((TDomainEvent)domainEvent, cancellationToken);
            }
        }
    }
}
