using System.Reflection;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.ArchitectureTests;

/// <summary>
/// Guards the dependency rule: dependencies point inwards (Api → Infrastructure → Application → Domain).
/// </summary>
public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(ShortUrl).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    [Theory]
    [InlineData("UrlShortener.Application")]
    [InlineData("UrlShortener.Infrastructure")]
    [InlineData("UrlShortener.Api")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("Microsoft.Extensions")]
    public void Domain_DoesNotDependOn(string forbiddenAssemblyPrefix) =>
        AssertNoDependency(DomainAssembly, forbiddenAssemblyPrefix);

    [Theory]
    [InlineData("UrlShortener.Infrastructure")]
    [InlineData("UrlShortener.Api")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.AspNetCore")]
    public void Application_DoesNotDependOn(string forbiddenAssemblyPrefix) =>
        AssertNoDependency(ApplicationAssembly, forbiddenAssemblyPrefix);

    [Fact]
    public void Infrastructure_DoesNotDependOnApi() =>
        AssertNoDependency(InfrastructureAssembly, "UrlShortener.Api");

    private static void AssertNoDependency(Assembly assembly, string forbiddenAssemblyPrefix)
    {
        var offending = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && name.StartsWith(forbiddenAssemblyPrefix, StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offending);
    }
}
