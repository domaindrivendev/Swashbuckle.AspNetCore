using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-AuthResponsesOperationFilter
public class AuthResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>()
            .Any() ?? false;

        if (hasAuthAttributes)
        {
            operation.Responses ??= [];
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        }
    }
}
// end-snippet
