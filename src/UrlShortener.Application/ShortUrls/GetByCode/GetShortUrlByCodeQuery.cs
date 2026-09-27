using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.ShortUrls.GetByCode;

public sealed record GetShortUrlByCodeQuery(string Code) : IQuery<ShortUrlResponse>;
