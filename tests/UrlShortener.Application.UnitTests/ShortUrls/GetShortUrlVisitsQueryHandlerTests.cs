using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.ShortUrls.GetVisits;
using UrlShortener.Application.UnitTests.Fakes;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Application.UnitTests.ShortUrls;

public class GetShortUrlVisitsQueryHandlerTests
{
    private readonly InMemoryShortUrlRepository _repository = new();

    [Fact]
    public async Task Handle_WithExistingCode_ReturnsVisitsPage()
    {
        _repository.Add(TestData.CreateShortUrl("abcd123"));
        var visits = new StubVisitReader
        {
            Visits =
            [
                new VisitResponse(Guid.CreateVersion7(), TestData.Now),
                new VisitResponse(Guid.CreateVersion7(), TestData.Now.AddMinutes(-5))
            ]
        };

        var result = await CreateHandler(visits).Handle(new GetShortUrlVisitsQuery("abcd123", 1, 20), default);

        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2L, result.Value.TotalCount);
        Assert.False(result.Value.HasNextPage);
    }

    [Fact]
    public async Task Handle_WithUnknownCode_ReturnsNotFound()
    {
        var result = await CreateHandler(new StubVisitReader()).Handle(new GetShortUrlVisitsQuery("missing1", 1, 20), default);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithInvalidPaging_ReturnsValidationError()
    {
        _repository.Add(TestData.CreateShortUrl("abcd123"));

        var result = await CreateHandler(new StubVisitReader())
            .Handle(new GetShortUrlVisitsQuery("abcd123", 1, PageRequest.MaxPageSize + 1), default);

        Assert.Equal(PageRequest.InvalidPageSize, result.Error);
    }

    private GetShortUrlVisitsQueryHandler CreateHandler(IVisitReader visitReader) => new(_repository, visitReader);
}
