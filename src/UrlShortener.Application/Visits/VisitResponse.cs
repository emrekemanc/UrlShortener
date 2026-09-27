namespace UrlShortener.Application.Visits;

public sealed record VisitResponse(Guid Id, DateTimeOffset VisitedAtUtc);
