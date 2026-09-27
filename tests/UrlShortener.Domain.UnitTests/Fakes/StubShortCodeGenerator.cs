using UrlShortener.Domain.ShortUrls;

namespace UrlShortener.Domain.UnitTests.Fakes;

internal sealed class StubShortCodeGenerator(params string[] codes) : IShortCodeGenerator
{
    private readonly Queue<string> _codes = new(codes);

    public ShortCode Generate() => ShortCode.Create(_codes.Dequeue()).Value;
}
