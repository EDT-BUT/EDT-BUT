using Edt.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Edt.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> GetActionResult<T>(this Result<T> result) =>
        result.IsSuccess ? new OkObjectResult(result.Value) : ToProblem(result.Error);

    public static ActionResult GetActionResult<T>(this Result<T> result, Func<T, ActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : ToProblem(result.Error);

    private static ObjectResult ToProblem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = TitleOf(error.Type),
            Detail = error.Message,
            Extensions = { ["code"] = error.Code },
        };

        return new ObjectResult(problem) { StatusCode = status };
    }

    private static string TitleOf(ErrorType type) => type switch
    {
        ErrorType.Validation => "Requête invalide",
        ErrorType.NotFound => "Introuvable",
        ErrorType.Unavailable => "Service indisponible",
        _ => "Erreur interne",
    };
}
