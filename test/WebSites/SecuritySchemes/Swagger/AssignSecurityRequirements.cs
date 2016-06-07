using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace SecuritySchemes.Swagger
{
    public class AssignSecurityRequirements : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            // Correspond each "Authorize" role to an oauth2 scope
            var controllerScopes = context.ApiDescription.GetControllerAttributes()
                .OfType<AuthorizeAttribute>()
                .SelectMany(attr => attr.Roles.Split(','));

            var actionScopes = context.ApiDescription.GetActionAttributes()
                .OfType<AuthorizeAttribute>()
                .SelectMany(attr => attr.Roles.Split(','));

            var scopes = controllerScopes.Union(actionScopes).Distinct();

            if (scopes.Any())
            {
                if (operation.Security == null)
                    operation.Security = new List<IDictionary<string, IEnumerable<string>>>();

                var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", scopes }
                };

                operation.Security.Add(oAuthRequirements);
            }
        }
    }
}