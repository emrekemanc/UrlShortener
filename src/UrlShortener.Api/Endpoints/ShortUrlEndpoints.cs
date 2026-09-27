using Microsoft.AspNetCore.Http.HttpResults;
using UrlShortener.Api.Contracts;
using UrlShortener.Api.Extensions;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.Paging;
using UrlShortener.Application.ShortUrls;
using UrlShortener.Application.ShortUrls.Create;
using UrlShortener.Application.ShortUrls.Deactivate;
using UrlShortener.Application.ShortUrls.GetAll;
using UrlShortener.Application.ShortUrls.GetByCode;
using UrlShortener.Application.ShortUrls.GetVisits;
using UrlShortener.Application.ShortUrls.Resolve;
using UrlShortener.Application.Visits;

namespace UrlShortener.Api.Endpoints;

internal static class ShortUrlEndpoints
{
    public static IEndpointRouteBuilder MapShortUrlEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/short-urls").WithTags("Short URLs");

        api.MapPost("/", CreateAsync).WithName("CreateShortUrl");
        api.MapGet("/", GetAllAsync).WithName("GetShortUrls");
        api.MapGet("/{code}", GetByCodeAsync).WithName("GetShortUrl");
        api.MapGet("/{code}/visits", GetVisitsAsync).WithName("GetShortUrlVisits");
        api.MapDelete("/{code}", DeactivateAsync).WithName("DeactivateShortUrl");

        app.MapGet("/{code}", RedirectAsync).WithName("RedirectToOriginalUrl").WithTags("Redirect");

        return app;
    }

    private static async Task<Results<Created<ShortUrlResource>, ProblemHttpResult>> CreateAsync(
        CreateShortUrlRequest request,
        ICommandHandler<CreateShortUrlCommand, ShortUrlResponse> handler,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var command = new CreateShortUrlCommand(request.Url, request.CustomCode, request.ExpiresAtUtc);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        var resource = ShortUrlResource.From(result.Value, httpRequest);
        return TypedResults.Created($"/api/short-urls/{resource.Code}", resource);
    }

    private static async Task<Results<Ok<PagedResponse<ShortUrlResource>>, ProblemHttpResult>> GetAllAsync(
        IQueryHandler<GetShortUrlsQuery, PagedResponse<ShortUrlResponse>> handler,
        HttpRequest httpRequest,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = PageRequest.DefaultPageSize)
    {
        var result = await handler.Handle(new GetShortUrlsQuery(page, pageSize), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value.Map(shortUrl => ShortUrlResource.From(shortUrl, httpRequest)))
            : result.Error.ToProblem();
    }

    private static async Task<Results<Ok<ShortUrlResource>, ProblemHttpResult>> GetByCodeAsync(
        string code,
        IQueryHandler<GetShortUrlByCodeQuery, ShortUrlResponse> handler,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetShortUrlByCodeQuery(code), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(ShortUrlResource.From(result.Value, httpRequest))
            : result.Error.ToProblem();
    }

    private static async Task<Results<Ok<PagedResponse<VisitResponse>>, ProblemHttpResult>> GetVisitsAsync(
        string code,
        IQueryHandler<GetShortUrlVisitsQuery, PagedResponse<VisitResponse>> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = PageRequest.DefaultPageSize)
    {
        var result = await handler.Handle(new GetShortUrlVisitsQuery(code, page, pageSize), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.Error.ToProblem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeactivateAsync(
        string code,
        ICommandHandler<DeactivateShortUrlCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeactivateShortUrlCommand(code), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblem();
    }

    private static async Task<Results<RedirectHttpResult, ProblemHttpResult>> RedirectAsync(
        string code,
        ICommandHandler<ResolveShortUrlCommand, string> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ResolveShortUrlCommand(code), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Redirect(result.Value)
            : result.Error.ToProblem();
    }
}
