using UrlShortener.Domain.Visits;

namespace UrlShortener.Application.UnitTests.Fakes;

internal sealed class InMemoryVisitRepository : IVisitRepository
{
    private readonly List<Visit> _visits = [];

    public IReadOnlyList<Visit> Visits => _visits;

    public void Add(Visit visit) => _visits.Add(visit);
}
