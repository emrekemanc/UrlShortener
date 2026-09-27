namespace UrlShortener.Application.Visits;

public sealed record VisitStatistics(long TotalVisits, DateTimeOffset? LastVisitedAtUtc)
{
    public static readonly VisitStatistics None = new(0, null);
}
