using UrlShortener.Application.Abstractions.Paging;

namespace UrlShortener.Application.UnitTests.Paging;

public class PagingTests
{
    [Fact]
    public void PageRequest_WithValidValues_CalculatesSkip()
    {
        var page = PageRequest.Create(3, 20).Value;

        Assert.Equal(40, page.Skip);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(PageRequest.MaxPage + 1)]
    public void PageRequest_WithPageOutOfRange_Fails(int page)
    {
        var result = PageRequest.Create(page, PageRequest.DefaultPageSize);

        Assert.Equal(PageRequest.InvalidPage, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(PageRequest.MaxPageSize + 1)]
    public void PageRequest_WithPageSizeOutOfRange_Fails(int pageSize)
    {
        var result = PageRequest.Create(1, pageSize);

        Assert.Equal(PageRequest.InvalidPageSize, result.Error);
    }

    [Theory]
    [InlineData(1, 10, 25, 3, true)]
    [InlineData(3, 10, 25, 3, false)]
    [InlineData(1, 10, 0, 0, false)]
    public void PagedResponse_CalculatesTotalPagesAndNextPage(
        int page,
        int pageSize,
        long totalCount,
        int expectedTotalPages,
        bool expectedHasNextPage)
    {
        var response = new PagedResponse<int>([], page, pageSize, totalCount);

        Assert.Equal(expectedTotalPages, response.TotalPages);
        Assert.Equal(expectedHasNextPage, response.HasNextPage);
    }
}
