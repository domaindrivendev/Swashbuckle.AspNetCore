using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OAuth2Integration.ResourceServer.Swagger
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        private readonly AuthorizationOptions _authorizationOptions;
        
        public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
        {
            // Beware: This might only part of the truth. If someone exchanges the IAuthorizationPolicyProvider and that loads
            // policies and requirements from another source than the configured options, we might not get all requirements
            // from here. But then we would have to make asynchronous calls from this synchronous interface.
            _authorizationOptions = authorizationOptions?.Value ?? throw new ArgumentNullException(nameof(authorizationOptions));
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
                .SelectMany(r => r.Requirements.OfType<ClaimsAuthorizationRequirement>())
                .Where(cr => cr.ClaimType == "scope")
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
