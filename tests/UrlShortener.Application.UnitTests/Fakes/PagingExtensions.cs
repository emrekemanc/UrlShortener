using UrlShortener.Application.Abstractions.Paging;

namespace UrlShortener.Application.UnitTests.Fakes;

internal static class PagingExtensions
{
    public static PagedResponse<T> ToPage<T>(this IReadOnlyList<T> source, PageRequest page) =>
        new([.. source.Skip(page.Skip).Take(page.PageSize)], page.Page, page.PageSize, source.Count);
}
