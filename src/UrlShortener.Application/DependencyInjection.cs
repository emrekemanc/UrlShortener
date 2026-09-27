using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Abstractions.Events;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application;

public static class DependencyInjection
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
        typeof(IDomainEventHandler<>)
    ];

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var implementationTypes = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (var implementationType in implementationTypes)
        {
            foreach (var serviceType in implementationType.GetInterfaces().Where(IsHandlerInterface))
            {
                services.AddScoped(serviceType, implementationType);
            }
        }

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ShortCodeAllocator>();

        return services;
    }

    private static bool IsHandlerInterface(Type type) =>
        type.IsGenericType && HandlerInterfaces.Contains(type.GetGenericTypeDefinition());
}
