using FGames.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Users.Api.Controllers;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return ToProblem(result);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ToProblem(result);
    }

    private static IActionResult ToProblem(Result result)
    {
        var code = result.FirstError.Code;

        var statusCode = code switch
        {
            "User.EmailAlreadyRegistered" => StatusCodes.Status409Conflict,
            "User.InvalidCredentials" or "User.NotActive" => StatusCodes.Status401Unauthorized,
            _ when code.EndsWith("NotFound", StringComparison.Ordinal) => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        return new ObjectResult(new { errors = result.Errors })
        {
            StatusCode = statusCode
        };
    }
}
