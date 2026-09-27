using UrlShortener.Domain.ShortUrls;
using UrlShortener.Domain.ShortUrls.Events;

namespace UrlShortener.Domain.UnitTests.ShortUrls;

public class ShortUrlTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly ShortCode Code = ShortCode.Create("abc1234").Value;
    private static readonly OriginalUrl Url = OriginalUrl.Create("https://example.com").Value;

    [Fact]
    public void Create_WithValidData_ReturnsActiveShortUrl()
    {
        var shortUrl = ShortUrl.Create(Code, Url, expiresAtUtc: null, Now).Value;

        Assert.Equal(Code, shortUrl.Code);
        Assert.Equal(Url, shortUrl.OriginalUrl);
        Assert.Equal(Now, shortUrl.CreatedAtUtc);
        Assert.Equal(0L, shortUrl.VisitCount);
        Assert.Equal(ShortUrlStatus.Active, shortUrl.GetStatus(Now));
    }

    [Fact]
    public void Create_RaisesShortUrlCreatedDomainEvent()
    {
        var shortUrl = ShortUrl.Create(Code, Url, expiresAtUtc: null, Now).Value;

        var domainEvent = Assert.IsType<ShortUrlCreatedDomainEvent>(Assert.Single(shortUrl.DomainEvents));
        Assert.Equal(shortUrl.Id, domainEvent.ShortUrlId);
        Assert.Equal(Code.Value, domainEvent.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-60)]
    public void Create_WithExpirationNotInFuture_Fails(int offsetMinutes)
    {
        var result = ShortUrl.Create(Code, Url, Now.AddMinutes(offsetMinutes), Now);

        Assert.Equal(ShortUrlErrors.ExpirationInPast, result.Error);
    }

    [Fact]
    public void Visit_WhenActive_ReturnsOriginalUrlAndRecordsVisit()
    {
        var shortUrl = CreateShortUrl();
        var visitedAt = Now.AddMinutes(5);

        var result = shortUrl.Visit(visitedAt);

        Assert.Equal(Url, result.Value);
        Assert.Equal(1L, shortUrl.VisitCount);
        Assert.Equal(visitedAt, shortUrl.LastVisitedAtUtc);
        Assert.IsType<ShortUrlVisitedDomainEvent>(Assert.Single(shortUrl.DomainEvents));
    }

    [Fact]
    public void Visit_AfterExpiration_ReturnsExpiredAndDoesNotCount()
    {
        var shortUrl = CreateShortUrl(expiresAtUtc: Now.AddHours(1));

        var result = shortUrl.Visit(Now.AddHours(2));

        Assert.Equal(ShortUrlErrors.Expired, result.Error);
        Assert.Equal(0L, shortUrl.VisitCount);
        Assert.Equal(ShortUrlStatus.Expired, shortUrl.GetStatus(Now.AddHours(2)));
    }

    [Fact]
    public void Visit_WhenDeactivated_ReturnsDeactivated()
    {
        var shortUrl = CreateShortUrl();
        shortUrl.Deactivate(Now);

        var result = shortUrl.Visit(Now.AddMinutes(1));

        Assert.Equal(ShortUrlErrors.Deactivated, result.Error);
    }

    [Fact]
    public void Deactivate_WhenActive_SetsDeactivationAndRaisesEvent()
    {
        var shortUrl = CreateShortUrl();

        var result = shortUrl.Deactivate(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, shortUrl.DeactivatedAtUtc);
        Assert.Equal(ShortUrlStatus.Deactivated, shortUrl.GetStatus(Now));
        Assert.IsType<ShortUrlDeactivatedDomainEvent>(Assert.Single(shortUrl.DomainEvents));
    }

    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_Fails()
    {
        var shortUrl = CreateShortUrl();
        shortUrl.Deactivate(Now);

        var result = shortUrl.Deactivate(Now.AddMinutes(1));

        Assert.Equal(ShortUrlErrors.AlreadyDeactivated, result.Error);
    }

    private static ShortUrl CreateShortUrl(DateTimeOffset? expiresAtUtc = null)
    {
        var shortUrl = ShortUrl.Create(Code, Url, expiresAtUtc, Now).Value;
        shortUrl.ClearDomainEvents();
        return shortUrl;
    }
}
