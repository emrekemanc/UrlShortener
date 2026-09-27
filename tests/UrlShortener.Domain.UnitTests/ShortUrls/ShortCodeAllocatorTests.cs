using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;
using UrlShortener.Domain.UnitTests.Fakes;

namespace UrlShortener.Domain.UnitTests.ShortUrls;

public class ShortCodeAllocatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryShortUrlRepository _repository = new();

    [Fact]
    public async Task AllocateAsync_WithoutRequestedCode_ReturnsGeneratedCode()
    {
        var allocator = CreateAllocator("gen1234");

        var result = await allocator.AllocateAsync(requestedCode: null);

        Assert.Equal("gen1234", result.Value.Value);
    }

    [Fact]
    public async Task AllocateAsync_WhenGeneratedCodeIsTaken_TriesNextCode()
    {
        AddExisting("taken01");
        var allocator = CreateAllocator("taken01", "fresh01");

        var result = await allocator.AllocateAsync(requestedCode: null);

        Assert.Equal("fresh01", result.Value.Value);
    }

    [Fact]
    public async Task AllocateAsync_WhenEveryGeneratedCodeIsTaken_Fails()
    {
        AddExisting("taken01");
        var allocator = CreateAllocator([.. Enumerable.Repeat("taken01", ShortCodeAllocator.MaxGenerationAttempts)]);

        var result = await allocator.AllocateAsync(requestedCode: null);

        Assert.Equal(ShortUrlErrors.CodeGenerationFailed, result.Error);
    }

    [Fact]
    public async Task AllocateAsync_WithFreeRequestedCode_ReturnsIt()
    {
        var allocator = CreateAllocator();

        var result = await allocator.AllocateAsync("my-link");

        Assert.Equal("my-link", result.Value.Value);
    }

    [Fact]
    public async Task AllocateAsync_WithTakenRequestedCode_ReturnsConflict()
    {
        AddExisting("my-link");
        var allocator = CreateAllocator();

        var result = await allocator.AllocateAsync("my-link");

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task AllocateAsync_WithInvalidRequestedCode_ReturnsValidationError()
    {
        var allocator = CreateAllocator();

        var result = await allocator.AllocateAsync("no spaces");

        Assert.Equal(ShortUrlErrors.InvalidCodeCharacters, result.Error);
    }

    private ShortCodeAllocator CreateAllocator(params string[] generatedCodes) =>
        new(_repository, new StubShortCodeGenerator(generatedCodes));

    private void AddExisting(string code) =>
        _repository.Add(ShortUrl.Create(
            ShortCode.Create(code).Value,
            OriginalUrl.Create("https://example.com").Value,
            expiresAtUtc: null,
            Now).Value);
}
