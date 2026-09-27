using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.ShortUrls.Create;
using UrlShortener.Application.UnitTests.Fakes;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.ShortUrls;

public class CreateShortUrlCommandHandlerTests
{
    private readonly InMemoryShortUrlRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);

    [Fact]
    public async Task Handle_WithValidUrl_PersistsShortUrlWithAllocatedCode()
    {
        var handler = CreateHandler("gen1234");

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, null, null), default);

        Assert.Equal("gen1234", result.Value.Code);
        Assert.Equal(TestData.Url, result.Value.OriginalUrl);
        Assert.Equal(nameof(ShortUrlStatus.Active), result.Value.Status);
        Assert.Equal(0L, result.Value.VisitCount);
        Assert.Single(_repository.ShortUrls);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCodeCannotBeAllocated_ReturnsErrorAndDoesNotSave()
    {
        _repository.Add(TestData.CreateShortUrl("my-link"));
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, "my-link", null), default);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidUrl_ReturnsValidationErrorAndDoesNotSave()
    {
        var handler = CreateHandler("gen1234");

        var result = await handler.Handle(new CreateShortUrlCommand("not-a-url", null, null), default);

        Assert.Equal(ShortUrlErrors.InvalidUrl, result.Error);
        Assert.Empty(_repository.ShortUrls);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithPastExpiration_ReturnsValidationError()
    {
        var handler = CreateHandler("gen1234");

        var result = await handler.Handle(
            new CreateShortUrlCommand(TestData.Url, null, TestData.Now.AddDays(-1)),
            default);

        Assert.Equal(ShortUrlErrors.ExpirationInPast, result.Error);
    }

    [Fact]
    public async Task Handle_WhenConcurrentInsertWinsTheCode_ReturnsConflict()
    {
        _unitOfWork.ExceptionToThrow = new UniqueConstraintViolationException(new InvalidOperationException());
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, "race-me", null), default);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    private CreateShortUrlCommandHandler CreateHandler(params string[] generatedCodes) =>
        new(
            _repository,
            new ShortCodeAllocator(_repository, new StubShortCodeGenerator(generatedCodes)),
            _unitOfWork,
            _timeProvider);
}
