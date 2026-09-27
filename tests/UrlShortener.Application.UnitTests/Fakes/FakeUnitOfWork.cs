using UrlShortener.Application.Abstractions.Data;

namespace UrlShortener.Application.UnitTests.Fakes;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Exception? ExceptionToThrow { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(1);
    }
}
