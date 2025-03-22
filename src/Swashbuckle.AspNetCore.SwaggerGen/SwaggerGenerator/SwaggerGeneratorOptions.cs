#if NET
using Microsoft.AspNetCore.Http.Metadata;
#endif
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
#if NET
using Microsoft.AspNetCore.Routing;
#endif
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerGeneratorOptions
{
    public SwaggerGeneratorOptions()
    {
        SwaggerDocs = new Dictionary<string, OpenApiInfo>();
        DocInclusionPredicate = DefaultDocInclusionPredicate;
        OperationIdSelector = DefaultOperationIdSelector;
        TagsSelector = DefaultTagsSelector;
        SortKeySelector = DefaultSortKeySelector;
        PathGroupSelector = DefaultPathGroupSelector;
        SecuritySchemesSelector = null;
        SchemaComparer = StringComparer.Ordinal;
        Servers = [];
        SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>();
        SecurityRequirements = [];
        ParameterFilters = [];
        ParameterAsyncFilters = [];
        RequestBodyFilters = [];
        RequestBodyAsyncFilters = [];
        OperationFilters = [];
        OperationAsyncFilters = [];
        DocumentFilters = [];
        DocumentAsyncFilters = [];
    }

    public IDictionary<string, OpenApiInfo> SwaggerDocs { get; set; }

    public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; }

    public bool IgnoreObsoleteActions { get; set; }

    public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; set; }

    public Func<ApiDescription, string> OperationIdSelector { get; set; }

    public Func<ApiDescription, IList<string>> TagsSelector { get; set; }

    public Func<ApiDescription, string> SortKeySelector { get; set; }

    public Func<ApiDescription, string> PathGroupSelector { get; set; }

    public bool InferSecuritySchemes { get; set; }

    public Func<IEnumerable<AuthenticationScheme>, IDictionary<string, OpenApiSecurityScheme>> SecuritySchemesSelector { get; set; }

    public bool DescribeAllParametersInCamelCase { get; set; }

    public List<OpenApiServer> Servers { get; set; }

    public IDictionary<string, OpenApiSecurityScheme> SecuritySchemes { get; set; }

    public IList<OpenApiSecurityRequirement> SecurityRequirements { get; set; }

    public IComparer<string> SchemaComparer { get; set; }

    public IList<IParameterFilter> ParameterFilters { get; set; }

    public IList<IParameterAsyncFilter> ParameterAsyncFilters { get; set; }

    public List<IRequestBodyFilter> RequestBodyFilters { get; set; }

    public IList<IRequestBodyAsyncFilter> RequestBodyAsyncFilters { get; set; }

    public List<IOperationFilter> OperationFilters { get; set; }

    public IList<IOperationAsyncFilter> OperationAsyncFilters { get; set; }

    public IList<IDocumentFilter> DocumentFilters { get; set; }

    public IList<IDocumentAsyncFilter> DocumentAsyncFilters { get; set; }

    public string XmlCommentEndOfLine { get; set; }

    private bool DefaultDocInclusionPredicate(string documentName, ApiDescription apiDescription)
    {
        return apiDescription.GroupName == null || apiDescription.GroupName == documentName;
    }

    private string DefaultOperationIdSelector(ApiDescription apiDescription)
    {
        var actionDescriptor = apiDescription.ActionDescriptor;

        // Resolve the operation ID from the route name and fallback to the
        // endpoint name if no route name is available. This allows us to
        // generate operation IDs for endpoints that are defined using
        // minimal APIs.
#if NET
        return
            actionDescriptor.AttributeRouteInfo?.Name
            ?? (actionDescriptor.EndpointMetadata?.LastOrDefault(m => m is IEndpointNameMetadata) as IEndpointNameMetadata)?.EndpointName;
#else
        return actionDescriptor.AttributeRouteInfo?.Name;
#endif
    }

    private IList<string> DefaultTagsSelector(ApiDescription apiDescription)
    {
#if !NET
        return [apiDescription.ActionDescriptor.RouteValues["controller"]];
#else
        var actionDescriptor = apiDescription.ActionDescriptor;
        if (actionDescriptor.EndpointMetadata?.LastOrDefault(m => m is ITagsMetadata) is ITagsMetadata metadata)
        {
            return [.. metadata.Tags];
        }

        return [apiDescription.ActionDescriptor.RouteValues["controller"]];
#endif
    }

    private string DefaultSortKeySelector(ApiDescription apiDescription)
    {
        return TagsSelector(apiDescription).First();
    }

    private static string DefaultPathGroupSelector(ApiDescription apiDescription)
    {
        return apiDescription.RelativePathSansParameterConstraints();
    }
}
