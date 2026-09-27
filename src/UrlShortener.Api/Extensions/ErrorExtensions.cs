using Microsoft.AspNetCore.Http.HttpResults;
using UrlShortener.Domain.Abstractions;

namespace UrlShortener.Api.Extensions;

internal static class ErrorExtensions
{
    public static ProblemHttpResult ToProblem(this Error error)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.Gone => (StatusCodes.Status410Gone, "Gone"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        return TypedResults.Problem(
            detail: error.Description,
            statusCode: statusCode,
            title: title,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code });
    }
}
