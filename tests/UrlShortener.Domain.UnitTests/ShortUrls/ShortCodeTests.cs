using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Domain.UnitTests.ShortUrls;

public class ShortCodeTests
{
    [Theory]
    [InlineData("abcd")]
    [InlineData("A1b2C3d")]
    [InlineData("my-link_2026")]
    public void Create_WithValidValue_Succeeds(string value)
    {
        var result = ShortCode.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value.Value);
    }

    [Fact]
    public void Create_TrimsSurroundingWhitespace()
    {
        var result = ShortCode.Create("  abcd  ");

        Assert.Equal("abcd", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyValue_Fails(string? value)
    {
        var result = ShortCode.Create(value);

        Assert.Equal(ShortUrlErrors.EmptyCode, result.Error);
    }

    [Theory]
    [InlineData(ShortCode.MinLength - 1)]
    [InlineData(ShortCode.MaxLength + 1)]
    public void Create_WithInvalidLength_Fails(int length)
    {
        var result = ShortCode.Create(new string('a', length));

        Assert.Equal(ShortUrlErrors.InvalidCodeLength, result.Error);
    }

    [Theory]
    [InlineData("hello world")]
    [InlineData("şehir")]
    [InlineData("a/b/c")]
    [InlineData("abc?")]
    public void Create_WithInvalidCharacters_Fails(string value)
    {
        var result = ShortCode.Create(value);

        Assert.Equal(ShortUrlErrors.InvalidCodeCharacters, result.Error);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("HEALTH")]
    [InlineData("openapi")]
    public void Create_WithReservedWord_Fails(string value)
    {
        var result = ShortCode.Create(value);

        Assert.Equal(ShortUrlErrors.ReservedCode, result.Error);
    }

    [Fact]
    public void ShortCodes_WithSameValue_AreEqual()
    {
        Assert.Equal(ShortCode.Create("abcd").Value, ShortCode.Create("abcd").Value);
    }
}
