using UrlShortener.Application.ShortUrls.Resolve;
using UrlShortener.Application.UnitTests.Fakes;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.ShortUrls;

public class ResolveShortUrlCommandHandlerTests
{
    private readonly InMemoryShortUrlRepository _shortUrlRepository = new();
    private readonly InMemoryVisitRepository _visitRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);

    [Fact]
    public async Task Handle_WithActiveCode_ReturnsOriginalUrlAndRecordsVisit()
    {
        var shortUrl = TestData.CreateShortUrl("abcd123");
        _shortUrlRepository.Add(shortUrl);

        var result = await CreateHandler().Handle(new ResolveShortUrlCommand("abcd123"), default);

        Assert.Equal(TestData.Url, result.Value);
        var visit = Assert.Single(_visitRepository.Visits);
        Assert.Equal(shortUrl.Id, visit.ShortUrlId);
        Assert.Equal(TestData.Now, visit.VisitedAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("missing1")]
    [InlineData("x")]
    [InlineData("not valid!")]
    public async Task Handle_WithUnknownOrInvalidCode_ReturnsNotFound(string code)
    {
        var result = await CreateHandler().Handle(new ResolveShortUrlCommand(code), default);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithExpiredCode_ReturnsExpiredAndRecordsNothing()
    {
        _shortUrlRepository.Add(TestData.CreateShortUrl("abcd123", expiresAtUtc: TestData.Now.AddHours(1)));
        _timeProvider.UtcNow = TestData.Now.AddHours(2);

        var result = await CreateHandler().Handle(new ResolveShortUrlCommand("abcd123"), default);

        Assert.Equal(ShortUrlErrors.Expired, result.Error);
        Assert.Empty(_visitRepository.Visits);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    private ResolveShortUrlCommandHandler CreateHandler() =>
        new(_shortUrlRepository, _visitRepository, _unitOfWork, _timeProvider);
}
