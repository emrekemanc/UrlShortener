using Microsoft.AspNetCore.Http.HttpResults;
using UrlShortener.Api.Contracts;
using UrlShortener.Api.Extensions;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.ShortUrls;
using UrlShortener.Application.ShortUrls.Create;
using UrlShortener.Application.ShortUrls.Deactivate;
using UrlShortener.Application.ShortUrls.GetByCode;
using UrlShortener.Application.ShortUrls.Visit;

namespace UrlShortener.Api.Endpoints;

internal static class ShortUrlEndpoints
{
    public static IEndpointRouteBuilder MapShortUrlEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/short-urls").WithTags("Short URLs");

        api.MapPost("/", CreateAsync).WithName("CreateShortUrl");
        api.MapGet("/{code}", GetByCodeAsync).WithName("GetShortUrl");
        api.MapDelete("/{code}", DeactivateAsync).WithName("DeactivateShortUrl");

        app.MapGet("/{code}", RedirectAsync).WithName("RedirectToOriginalUrl").WithTags("Redirect");

        return app;
    }

    private static async Task<Results<Created<ShortUrlResource>, ProblemHttpResult>> CreateAsync(
        CreateShortUrlRequest request,
        ICommandHandler<CreateShortUrlCommand, ShortUrlResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new CreateShortUrlCommand(request.Url, request.CustomCode, request.ExpiresAtUtc);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        var resource = ShortUrlResource.From(result.Value, httpContext.Request);
        return TypedResults.Created($"/api/short-urls/{resource.Code}", resource);
    }

    private static async Task<Results<Ok<ShortUrlResource>, ProblemHttpResult>> GetByCodeAsync(
        string code,
        IQueryHandler<GetShortUrlByCodeQuery, ShortUrlResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetShortUrlByCodeQuery(code), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(ShortUrlResource.From(result.Value, httpContext.Request))
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

    // 302 (not 301) so browsers don't cache the redirect and every visit is counted.
    private static async Task<Results<RedirectHttpResult, ProblemHttpResult>> RedirectAsync(
        string code,
        ICommandHandler<VisitShortUrlCommand, string> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new VisitShortUrlCommand(code), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Redirect(result.Value)
            : result.Error.ToProblem();
    }
}
