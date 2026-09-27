namespace UrlShortener.Domain.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}
