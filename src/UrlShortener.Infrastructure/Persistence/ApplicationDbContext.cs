using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Events;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Domain.Visits;

namespace UrlShortener.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEventDispatcher) : DbContext(options), IUnitOfWork
{
    private const int SqliteUniqueConstraintFailed = 2067;

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public DbSet<Visit> Visits => Set<Visit>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();

        int result;
        try
        {
            result = await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteUniqueConstraintFailed })
        {
            throw new UniqueConstraintViolationException(exception);
        }

        await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no DateTimeOffset type; storing it as a sortable integer keeps MAX and ORDER BY working.
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker.Entries<IAggregateRoot>().Select(entry => entry.Entity).ToList();
        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

        return domainEvents;
    }
}
