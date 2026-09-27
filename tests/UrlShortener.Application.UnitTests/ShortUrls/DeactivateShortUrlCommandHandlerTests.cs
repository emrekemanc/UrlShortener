using UrlShortener.Application.ShortUrls.Deactivate;
using UrlShortener.Application.UnitTests.Fakes;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.ShortUrls;

public class DeactivateShortUrlCommandHandlerTests
{
    private readonly InMemoryShortUrlRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);

    [Fact]
    public async Task Handle_WithActiveCode_DeactivatesAndSaves()
    {
        var shortUrl = TestData.CreateShortUrl("abcd123");
        _repository.Add(shortUrl);

        var result = await CreateHandler().Handle(new DeactivateShortUrlCommand("abcd123"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(ShortUrlStatus.Deactivated, shortUrl.GetStatus(TestData.Now));
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownCode_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new DeactivateShortUrlCommand("missing1"), default);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenAlreadyDeactivated_ReturnsConflict()
    {
        var shortUrl = TestData.CreateShortUrl("abcd123");
        shortUrl.Deactivate(TestData.Now);
        _repository.Add(shortUrl);

        var result = await CreateHandler().Handle(new DeactivateShortUrlCommand("abcd123"), default);

        Assert.Equal(ShortUrlErrors.AlreadyDeactivated, result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    private DeactivateShortUrlCommandHandler CreateHandler() => new(_repository, _unitOfWork, _timeProvider);
}
