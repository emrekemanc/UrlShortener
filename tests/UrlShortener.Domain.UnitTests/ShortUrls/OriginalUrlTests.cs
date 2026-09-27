using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Domain.UnitTests.ShortUrls;

public class OriginalUrlTests
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path?q=1#section")]
    [InlineData("https://sub.example.com:8443/a/b")]
    public void Create_WithAbsoluteHttpUrl_Succeeds(string value)
    {
        var result = OriginalUrl.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyValue_Fails(string? value)
    {
        var result = OriginalUrl.Create(value);

        Assert.Equal(ShortUrlErrors.EmptyUrl, result.Error);
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("/relative/path")]
    [InlineData("ftp://example.com/file.txt")]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:someone@example.com")]
    public void Create_WithNonHttpOrRelativeUrl_Fails(string value)
    {
        var result = OriginalUrl.Create(value);

        Assert.Equal(ShortUrlErrors.InvalidUrl, result.Error);
    }

    [Fact]
    public void Create_WithTooLongUrl_Fails()
    {
        var value = "https://example.com/" + new string('a', OriginalUrl.MaxLength);

        var result = OriginalUrl.Create(value);

        Assert.Equal(ShortUrlErrors.UrlTooLong, result.Error);
    }
}
