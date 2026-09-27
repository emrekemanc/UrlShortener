using UrlShortener.Domain.Visits;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.Repositories;

internal sealed class VisitRepository(ApplicationDbContext dbContext) : IVisitRepository
{
    public void Add(Visit visit) => dbContext.Visits.Add(visit);
}
