using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Application.Abstractions.Paging;

public sealed record PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MaxPage = int.MaxValue / MaxPageSize;

    public static readonly Error InvalidPage = Error.Validation(
        "Paging.InvalidPage",
        $"Page must be between 1 and {MaxPage}.");

    public static readonly Error InvalidPageSize = Error.Validation(
        "Paging.InvalidPageSize",
        $"Page size must be between 1 and {MaxPageSize}.");

    private PageRequest(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - 1) * PageSize;

    public static Result<PageRequest> Create(int page, int pageSize)
    {
        if (page is < 1 or > MaxPage)
        {
            return InvalidPage;
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            return InvalidPageSize;
        }

        return new PageRequest(page, pageSize);
    }
}
