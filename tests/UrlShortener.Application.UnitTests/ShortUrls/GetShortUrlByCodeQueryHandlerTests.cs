using UrlShortener.Application.ShortUrls.GetByCode;
using UrlShortener.Application.UnitTests.Fakes;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.ShortUrls;

public class GetShortUrlByCodeQueryHandlerTests
{
    private readonly InMemoryShortUrlRepository _repository = new();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);

    [Fact]
    public async Task Handle_WithExistingCode_ReturnsDetailsWithVisitStatistics()
    {
        _repository.Add(TestData.CreateShortUrl("abcd123"));
        var lastVisit = TestData.Now.AddMinutes(-3);
        var handler = CreateHandler(new VisitStatistics(42, lastVisit));

        var result = await handler.Handle(new GetShortUrlByCodeQuery("abcd123"), default);

        Assert.Equal("abcd123", result.Value.Code);
        Assert.Equal(42L, result.Value.VisitCount);
        Assert.Equal(lastVisit, result.Value.LastVisitedAtUtc);
    }

    [Fact]
    public async Task Handle_ReportsStatusAtCurrentTime()
    {
        _repository.Add(TestData.CreateShortUrl("abcd123", expiresAtUtc: TestData.Now.AddHours(1)));
        _timeProvider.UtcNow = TestData.Now.AddHours(2);

        var result = await CreateHandler(VisitStatistics.None).Handle(new GetShortUrlByCodeQuery("abcd123"), default);

        Assert.Equal(nameof(ShortUrlStatus.Expired), result.Value.Status);
    }

    [Fact]
    public async Task Handle_WithUnknownCode_ReturnsNotFound()
    {
        var result = await CreateHandler(VisitStatistics.None).Handle(new GetShortUrlByCodeQuery("missing1"), default);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    private GetShortUrlByCodeQueryHandler CreateHandler(VisitStatistics statistics) =>
        new(_repository, new StubVisitReader { Statistics = statistics }, _timeProvider);
}
