namespace UrlShortener.Domain.Abstractions;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    // Required by EF Core for materialization.
    protected Entity() => Id = default!;

    public TId Id { get; private init; }

    public bool Equals(Entity<TId>? other) =>
        other is not null &&
        GetType() == other.GetType() &&
        EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);
}
