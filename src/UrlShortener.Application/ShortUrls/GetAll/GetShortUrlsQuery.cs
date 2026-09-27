using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.Paging;

namespace UrlShortener.Application.ShortUrls.GetAll;

public sealed record GetShortUrlsQuery(int Page, int PageSize) : IQuery<PagedResponse<ShortUrlResponse>>;
