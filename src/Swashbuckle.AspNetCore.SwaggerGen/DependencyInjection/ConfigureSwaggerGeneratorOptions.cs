using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

internal class ConfigureSwaggerGeneratorOptions(
    IOptions<SwaggerGenOptions> swaggerGenOptionsAccessor,
    IServiceProvider serviceProvider,
    IWebHostEnvironment hostingEnv) : IConfigureOptions<SwaggerGeneratorOptions>
{
    private readonly SwaggerGenOptions _swaggerGenOptions = swaggerGenOptionsAccessor.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IWebHostEnvironment _hostingEnv = hostingEnv;

    public void Configure(SwaggerGeneratorOptions options)
    {
        DeepCopy(_swaggerGenOptions.SwaggerGeneratorOptions, options);

        // Create and add any filters that were specified through the FilterDescriptor lists ...

        foreach (var filterDescriptor in _swaggerGenOptions.ParameterFilterDescriptors)
        {
            if (filterDescriptor.IsAssignableTo(typeof(IParameterFilter)))
            {
                options.ParameterFilters.Add(GetOrCreateFilter<IParameterFilter>(filterDescriptor));
            }

            if (filterDescriptor.IsAssignableTo(typeof(IParameterAsyncFilter)))
            {
                options.ParameterAsyncFilters.Add(GetOrCreateFilter<IParameterAsyncFilter>(filterDescriptor));
            }
        }

        foreach (var filterDescriptor in _swaggerGenOptions.RequestBodyFilterDescriptors)
        {
            if (filterDescriptor.IsAssignableTo(typeof(IRequestBodyFilter)))
            {
                options.RequestBodyFilters.Add(GetOrCreateFilter<IRequestBodyFilter>(filterDescriptor));
            }

            if (filterDescriptor.IsAssignableTo(typeof(IRequestBodyAsyncFilter)))
            {
                options.RequestBodyAsyncFilters.Add(GetOrCreateFilter<IRequestBodyAsyncFilter>(filterDescriptor));
            }
        }

        foreach (var filterDescriptor in _swaggerGenOptions.OperationFilterDescriptors)
        {
            if (filterDescriptor.IsAssignableTo(typeof(IOperationFilter)))
            {
                options.OperationFilters.Add(GetOrCreateFilter<IOperationFilter>(filterDescriptor));
            }

            if (filterDescriptor.IsAssignableTo(typeof(IOperationAsyncFilter)))
            {
                options.OperationAsyncFilters.Add(GetOrCreateFilter<IOperationAsyncFilter>(filterDescriptor));
            }
        }

        foreach (var filterDescriptor in _swaggerGenOptions.DocumentFilterDescriptors)
        {
            if (filterDescriptor.IsAssignableTo(typeof(IDocumentFilter)))
            {
                options.DocumentFilters.Add(GetOrCreateFilter<IDocumentFilter>(filterDescriptor));
            }

            if (filterDescriptor.IsAssignableTo(typeof(IDocumentAsyncFilter)))
            {
                options.DocumentAsyncFilters.Add(GetOrCreateFilter<IDocumentAsyncFilter>(filterDescriptor));
            }
        }

        if (!options.SwaggerDocs.Any())
        {
            options.SwaggerDocs.Add("v1", new OpenApiInfo { Title = _hostingEnv.ApplicationName, Version = "1.0" });
        }
    }

    public static void DeepCopy(SwaggerGeneratorOptions source, SwaggerGeneratorOptions target)
    {
        target.SwaggerDocs = new Dictionary<string, OpenApiInfo>(source.SwaggerDocs);
        target.DocInclusionPredicate = source.DocInclusionPredicate;
        target.IgnoreObsoleteActions = source.IgnoreObsoleteActions;
        target.ConflictingActionsResolver = source.ConflictingActionsResolver;
        target.OperationIdSelector = source.OperationIdSelector;
        target.TagsSelector = source.TagsSelector;
        target.SortKeySelector = source.SortKeySelector;
        target.InferSecuritySchemes = source.InferSecuritySchemes;
        target.DescribeAllParametersInCamelCase = source.DescribeAllParametersInCamelCase;
        target.SchemaComparer = source.SchemaComparer;
        target.Servers = [.. source.Servers];
        target.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>(source.SecuritySchemes);
        target.SecurityRequirements = [.. source.SecurityRequirements];
        target.ParameterFilters = [.. source.ParameterFilters];
        target.ParameterAsyncFilters = [.. source.ParameterAsyncFilters];
        target.OperationFilters = [.. source.OperationFilters];
        target.OperationAsyncFilters = [.. source.OperationAsyncFilters];
        target.DocumentFilters = [.. source.DocumentFilters];
        target.DocumentAsyncFilters = [.. source.DocumentAsyncFilters];
        target.RequestBodyFilters = [.. source.RequestBodyFilters];
        target.RequestBodyAsyncFilters = [.. source.RequestBodyAsyncFilters];
        target.SecuritySchemesSelector = source.SecuritySchemesSelector;
        target.PathGroupSelector = source.PathGroupSelector;
        target.XmlCommentEndOfLine = source.XmlCommentEndOfLine;
    }

    private TFilter GetOrCreateFilter<TFilter>(FilterDescriptor filterDescriptor)
    {
        return (TFilter)(filterDescriptor.FilterInstance
            ?? ActivatorUtilities.CreateInstance(_serviceProvider, filterDescriptor.Type, filterDescriptor.Arguments));
    }
}
