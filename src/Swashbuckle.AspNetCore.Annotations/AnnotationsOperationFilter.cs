using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations;

public class AnnotationsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IEnumerable<object> controllerAttributes = [];
        IEnumerable<object> actionAttributes = [];
        IEnumerable<object> metadataAttributes = [];

        if (context.MethodInfo is { } methodInfo)
        {
            controllerAttributes = methodInfo.DeclaringType.GetCustomAttributes(true);
            actionAttributes = methodInfo.GetCustomAttributes(true);
        }

        if (context.ApiDescription?.ActionDescriptor?.EndpointMetadata is { } metadata)
        {
            metadataAttributes = metadata;
        }

        // NOTE: When controller and action attributes are applicable, action attributes should take priority.
        // Hence, why they're at the end of the list (i.e. last one wins).
        // Distinct() is applied due to an ASP.NET Core issue: https://github.com/dotnet/aspnetcore/issues/34199.
        var allAttributes = controllerAttributes
            .Union(actionAttributes)
            .Union(metadataAttributes)
            .Distinct();

        var actionAndEndpointAttributes = actionAttributes
            .Union(metadataAttributes)
            .Distinct();

        ApplySwaggerOperationAttribute(operation, actionAndEndpointAttributes);
        ApplySwaggerOperationFilterAttributes(operation, context, allAttributes);
        ApplySwaggerResponseAttributes(operation, context, allAttributes);
    }

    private static void ApplySwaggerOperationAttribute(
        OpenApiOperation operation,
        IEnumerable<object> actionAttributes)
    {
        var swaggerOperationAttribute = actionAttributes
            .OfType<SwaggerOperationAttribute>()
            .FirstOrDefault();

        if (swaggerOperationAttribute == null)
        {
            return;
        }

        if (swaggerOperationAttribute.Summary is { } summary)
        {
            operation.Summary = summary;
        }

        if (swaggerOperationAttribute.Description is { } description)
        {
            operation.Description = description;
        }

        if (swaggerOperationAttribute.OperationId is { } operationId)
        {
            operation.OperationId = operationId;
        }

        if (swaggerOperationAttribute.Tags is { } tags)
        {
            operation.Tags = new SortedSet<OpenApiTagReference>(tags.Select(tagName => new OpenApiTagReference(tagName)));
        }
    }

    public static void ApplySwaggerOperationFilterAttributes(
        OpenApiOperation operation,
        OperationFilterContext context,
        IEnumerable<object> controllerAndActionAttributes)
    {
        var swaggerOperationFilterAttributes = controllerAndActionAttributes
            .OfType<SwaggerOperationFilterAttribute>();

        foreach (var swaggerOperationFilterAttribute in swaggerOperationFilterAttributes)
        {
            var filter = (IOperationFilter)Activator.CreateInstance(swaggerOperationFilterAttribute.FilterType);
            filter.Apply(operation, context);
        }
    }

    private static void ApplySwaggerResponseAttributes(
        OpenApiOperation operation,
        OperationFilterContext context,
        IEnumerable<object> controllerAndActionAttributes)
    {
        var swaggerResponseAttributes = controllerAndActionAttributes.OfType<SwaggerResponseAttribute>();

        foreach (var swaggerResponseAttribute in swaggerResponseAttributes)
        {
            var statusCode = swaggerResponseAttribute.StatusCode.ToString();

            operation.Responses ??= [];

            if (!operation.Responses.TryGetValue(statusCode, out var response))
            {
                response = new OpenApiResponse();
            }

            if (swaggerResponseAttribute.Description is { } description)
            {
                response.Description = description;
            }

            operation.Responses[statusCode] = response;

            if (response is OpenApiResponse concrete &&
                swaggerResponseAttribute.ContentTypes is { } contentTypes)
            {
                concrete.Content?.Clear();
                concrete.Content ??= new Dictionary<string, OpenApiMediaType>();

                foreach (var contentType in contentTypes)
                {
                    var schema = (swaggerResponseAttribute.Type != null && swaggerResponseAttribute.Type != typeof(void))
                        ? context.SchemaGenerator.GenerateSchema(swaggerResponseAttribute.Type, context.SchemaRepository)
                        : null;

                    concrete.Content.Add(contentType, new OpenApiMediaType { Schema = schema });
                }
            }
        }
    }
}
