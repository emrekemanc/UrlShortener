using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.ShortUrls;
using UrlShortener.Application.ShortUrls.GetAll;
using UrlShortener.Application.UnitTests.Fakes;
using UrlShortener.Application.Visits;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Application.UnitTests.ShortUrls;

public class GetShortUrlsQueryHandlerTests
{
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);

    [Fact]
    public async Task Handle_ReturnsRequestedPageWithStatusAndVisits()
    {
        var lastVisit = TestData.Now.AddMinutes(-1);
        var reader = new StubShortUrlReader
        {
            ShortUrls =
            [
                new ShortUrlWithVisits(TestData.CreateShortUrl("first01"), new VisitStatistics(7, lastVisit)),
                new ShortUrlWithVisits(TestData.CreateShortUrl("second1"), VisitStatistics.None),
                new ShortUrlWithVisits(TestData.CreateShortUrl("third01"), VisitStatistics.None)
            ]
        };

        var result = await CreateHandler(reader).Handle(new GetShortUrlsQuery(1, 2), default);

        var page = result.Value;
        Assert.Equal(["first01", "second1"], page.Items.Select(item => item.Code));
        Assert.Equal(3L, page.TotalCount);
        Assert.True(page.HasNextPage);

        var first = page.Items[0];
        Assert.Equal(nameof(ShortUrlStatus.Active), first.Status);
        Assert.Equal(7L, first.VisitCount);
        Assert.Equal(lastVisit, first.LastVisitedAtUtc);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, PageRequest.MaxPageSize + 1)]
    public async Task Handle_WithInvalidPaging_ReturnsValidationError(int page, int pageSize)
    {
        var result = await CreateHandler(new StubShortUrlReader()).Handle(new GetShortUrlsQuery(page, pageSize), default);

        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    private GetShortUrlsQueryHandler CreateHandler(IShortUrlReader reader) => new(reader, _timeProvider);
}
