using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.ShortCodes;

namespace UrlShortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
        services.AddSingleton<IShortCodeGenerator, RandomShortCodeGenerator>();
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }

    /// <summary>
    /// Creates the database schema if it does not exist yet. Swap for EF Core migrations
    /// (<c>Database.MigrateAsync</c>) once the schema starts evolving.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
