using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.Visits;

namespace UrlShortener.Application.ShortUrls.GetVisits;

public sealed record GetShortUrlVisitsQuery(string Code, int Page, int PageSize) : IQuery<PagedResponse<VisitResponse>>;
