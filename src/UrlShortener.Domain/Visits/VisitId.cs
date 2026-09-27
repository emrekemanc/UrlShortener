namespace UrlShortener.Domain.Visits;

public readonly record struct VisitId(Guid Value)
{
    public static VisitId New() => new(Guid.CreateVersion7());
}
