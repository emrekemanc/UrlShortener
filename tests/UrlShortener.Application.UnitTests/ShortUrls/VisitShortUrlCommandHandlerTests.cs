using UrlShortener.Application.ShortUrls.Visit;
using UrlShortener.Application.UnitTests.Fakes;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.ShortUrls;

public class VisitShortUrlCommandHandlerTests
{
    private readonly InMemoryShortUrlRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);

    [Fact]
    public async Task Handle_WithActiveCode_ReturnsOriginalUrlAndSavesVisit()
    {
        var shortUrl = TestData.CreateShortUrl("abcd123");
        _repository.Add(shortUrl);

        var result = await CreateHandler().Handle(new VisitShortUrlCommand("abcd123"), default);

        Assert.Equal(TestData.Url, result.Value);
        Assert.Equal(1L, shortUrl.VisitCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("missing1")]
    [InlineData("x")]
    [InlineData("not valid!")]
    public async Task Handle_WithUnknownOrInvalidCode_ReturnsNotFound(string code)
    {
        var result = await CreateHandler().Handle(new VisitShortUrlCommand(code), default);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithExpiredCode_ReturnsExpiredAndDoesNotSave()
    {
        _repository.Add(TestData.CreateShortUrl("abcd123", expiresAtUtc: TestData.Now.AddHours(1)));
        _timeProvider.UtcNow = TestData.Now.AddHours(2);

        var result = await CreateHandler().Handle(new VisitShortUrlCommand("abcd123"), default);

        Assert.Equal(ShortUrlErrors.Expired, result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    private VisitShortUrlCommandHandler CreateHandler() => new(_repository, _unitOfWork, _timeProvider);
}
