using System.Reflection;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.ArchitectureTests;

public class DomainModelTests
{
    private static readonly Assembly DomainAssembly = typeof(ShortUrl).Assembly;

    [Fact]
    public void Entities_HaveNoPublicSetters()
    {
        // State may only change through behavior methods that protect the invariants.
        var violations = DomainAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && IsEntity(type))
            .SelectMany(type => type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.SetMethod?.IsPublic == true)
                .Select(property => $"{type.Name}.{property.Name}"))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Entities_HaveNoPublicConstructors()
    {
        // Aggregates are created through factory methods so they can never start in an invalid state.
        var violations = DomainAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && IsEntity(type))
            .Where(type => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length != 0)
            .Select(type => type.Name)
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void DomainEvents_AreSealed()
    {
        var violations = DomainAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true } && typeof(IDomainEvent).IsAssignableFrom(type) && !type.IsSealed)
            .Select(type => type.Name)
            .ToList();

        Assert.Empty(violations);
    }

    private static bool IsEntity(Type type)
    {
        for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Entity<>))
            {
                return true;
            }
        }

        return false;
    }
}
