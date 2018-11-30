using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OAuthSecurity.Swagger
{
    public class OAuthOperationFilter : IOperationFilter
    {
        private readonly AuthorizationOptions _authorizationOptions;
        
        public OAuthOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
        {
            _authorizationOptions = authorizationOptions.Value;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var requiredPolicies = context.MethodInfo
                .GetCustomAttributes(true)
                .Concat(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy)
                .Distinct();

            var requiredScopes = requiredPolicies.Select(p => _authorizationOptions.GetPolicy(p))
                .SelectMany(policy => policy.Requirements.OfType<ClaimsAuthorizationRequirement>())
                .Where(req => req.ClaimType == "scope")
                .SelectMany(r => r.AllowedValues)
                .Distinct()
                .ToList();

            if (requiredScopes.Any())
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                };

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ oAuthScheme ] = requiredScopes
                    }
                };
            }
        }
    }
}
