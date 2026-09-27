namespace UrlShortener.Application.Abstractions.Paging;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public PagedResponse<TResult> Map<TResult>(Func<T, TResult> map) =>
        new([.. Items.Select(map)], Page, PageSize, TotalCount);
}
