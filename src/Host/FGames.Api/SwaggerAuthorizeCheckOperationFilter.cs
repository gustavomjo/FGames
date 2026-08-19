using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FGames.Api;

public sealed class SwaggerAuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? [];
        var actionAttributes = context.MethodInfo.GetCustomAttributes(true);

        var isAnonymous = controllerAttributes.OfType<AllowAnonymousAttribute>().Any()
            || actionAttributes.OfType<AllowAnonymousAttribute>().Any();

        if (isAnonymous)
        {
            operation.Security.Clear();
            return;
        }

        var requiresAuthorization = controllerAttributes.OfType<AuthorizeAttribute>().Any()
            || actionAttributes.OfType<AuthorizeAttribute>().Any();

        if (!requiresAuthorization)
            operation.Security.Clear();
    }
}
