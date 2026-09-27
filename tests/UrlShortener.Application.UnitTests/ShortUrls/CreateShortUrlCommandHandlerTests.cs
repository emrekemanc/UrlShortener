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
    public async Task Handle_WithoutCustomCode_UsesGeneratedCode()
    {
        var handler = CreateHandler(new StubShortCodeGenerator("gen1234"));

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, null, null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("gen1234", result.Value.Code);
        Assert.Equal(TestData.Url, result.Value.OriginalUrl);
        Assert.Equal(nameof(ShortUrlStatus.Active), result.Value.Status);
        Assert.Single(_repository.ShortUrls);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenGeneratedCodeCollides_RetriesWithNextCode()
    {
        _repository.Add(TestData.CreateShortUrl("taken01"));
        var handler = CreateHandler(new StubShortCodeGenerator("taken01", "fresh01"));

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, null, null), default);

        Assert.Equal("fresh01", result.Value.Code);
    }

    [Fact]
    public async Task Handle_WhenEveryGeneratedCodeCollides_Fails()
    {
        _repository.Add(TestData.CreateShortUrl("taken01"));
        var codes = Enumerable.Repeat("taken01", CreateShortUrlCommandHandler.MaxGenerationAttempts).ToArray();
        var handler = CreateHandler(new StubShortCodeGenerator(codes));

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, null, null), default);

        Assert.Equal(ShortUrlErrors.CodeGenerationFailed, result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAvailableCustomCode_UsesCustomCode()
    {
        var handler = CreateHandler(new StubShortCodeGenerator());

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, "my-link", null), default);

        Assert.Equal("my-link", result.Value.Code);
    }

    [Fact]
    public async Task Handle_WithTakenCustomCode_ReturnsConflict()
    {
        _repository.Add(TestData.CreateShortUrl("my-link"));
        var handler = CreateHandler(new StubShortCodeGenerator());

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, "my-link", null), default);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidCustomCode_ReturnsValidationError()
    {
        var handler = CreateHandler(new StubShortCodeGenerator());

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, "no spaces", null), default);

        Assert.Equal(ShortUrlErrors.InvalidCodeCharacters, result.Error);
    }

    [Fact]
    public async Task Handle_WithInvalidUrl_ReturnsValidationErrorAndDoesNotSave()
    {
        var handler = CreateHandler(new StubShortCodeGenerator("gen1234"));

        var result = await handler.Handle(new CreateShortUrlCommand("not-a-url", null, null), default);

        Assert.Equal(ShortUrlErrors.InvalidUrl, result.Error);
        Assert.Empty(_repository.ShortUrls);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithPastExpiration_ReturnsValidationError()
    {
        var handler = CreateHandler(new StubShortCodeGenerator("gen1234"));

        var result = await handler.Handle(
            new CreateShortUrlCommand(TestData.Url, null, TestData.Now.AddDays(-1)),
            default);

        Assert.Equal(ShortUrlErrors.ExpirationInPast, result.Error);
    }

    [Fact]
    public async Task Handle_WhenConcurrentInsertWinsTheCode_ReturnsConflict()
    {
        _unitOfWork.ExceptionToThrow = new UniqueConstraintViolationException(new Exception());
        var handler = CreateHandler(new StubShortCodeGenerator());

        var result = await handler.Handle(new CreateShortUrlCommand(TestData.Url, "race-me", null), default);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    private CreateShortUrlCommandHandler CreateHandler(IShortCodeGenerator codeGenerator) =>
        new(_repository, codeGenerator, _unitOfWork, _timeProvider);
}
