using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Events;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEventDispatcher) : DbContext(options), IUnitOfWork
{
    private const int SqliteConstraintUnique = 2067;
    private const int SqliteConstraintPrimaryKey = 1555;

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();

        int result;
        try
        {
            result = await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException
        {
            SqliteExtendedErrorCode: SqliteConstraintUnique or SqliteConstraintPrimaryKey
        })
        {
            throw new UniqueConstraintViolationException(exception);
        }

        // Events are published only after the state change has been committed.
        await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count != 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

        return domainEvents;
    }
}
