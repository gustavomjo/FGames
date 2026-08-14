using System.Net;
using System.Security.Claims;
using FGames.Modules.Users.Domain.Enums;
using FGames.Modules.Users.Domain.Interfaces;

namespace FGames.Api.Middleware;

public sealed class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var userRepository = context.RequestServices.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetByIdAsync(userId, context.RequestAborted);

        if (user?.Status != UserStatus.Active)
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        await _next(context);
    }

    private static Task WriteUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["WWW-Authenticate"] = "Bearer";

        return context.Response.WriteAsJsonAsync(new
        {
            title = "Usuário inativo ou bloqueado.",
            status = (int)HttpStatusCode.Unauthorized
        });
    }
}
