using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.ShortUrls;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Domain.Visits;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Infrastructure.Queries;
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
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<IShortUrlReader, ShortUrlReader>();
        services.AddScoped<IVisitReader, VisitReader>();
        services.AddSingleton<IShortCodeGenerator, RandomShortCodeGenerator>();
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
