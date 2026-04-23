using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Authorization.Swagger;

public class SecurityRequirementsAsyncOperationFilter(IAuthorizationPolicyProvider authorizationPolicyProvider) : IOperationAsyncFilter
{
    public async Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
    {
        var authorizeAttributes = context.MethodInfo
            .GetCustomAttributes(true)
            .Concat(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>()
            .ToList();

        if (authorizeAttributes.Count == 0)
        {
            return;
        }

        var requiredRoles = new List<string>();

        foreach (var attribute in authorizeAttributes)
        {
            if (attribute.Policy == null)
            {
                continue;
            }

            var policy = await authorizationPolicyProvider.GetPolicyAsync(attribute.Policy);
            if (policy == null)
            {
                continue;
            }

            var roleRequirements = policy.Requirements
                .OfType<ClaimsAuthorizationRequirement>()
                .Where(r => r.ClaimType == ClaimTypes.Role)
                .SelectMany(r => r.AllowedValues);

            requiredRoles.AddRange(roleRequirements);
        }

        if (requiredRoles.Count == 0)
        {
            return;
        }

        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

        var scheme = new OpenApiSecuritySchemeReference("bearer", context.Document);

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [scheme] = requiredRoles.Distinct().ToList()
            }
        ];
    }
}
