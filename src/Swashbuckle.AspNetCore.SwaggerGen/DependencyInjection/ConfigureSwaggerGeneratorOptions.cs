using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

#if NETSTANDARD2_0
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#endif

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ConfigureSwaggerGeneratorOptions : IConfigureOptions<SwaggerGeneratorOptions>
    {
        private readonly SwaggerGenOptions _swaggerGenOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _hostingEnv;

        public ConfigureSwaggerGeneratorOptions(
            IOptions<SwaggerGenOptions> swaggerGenOptionsAccessor,
            IServiceProvider serviceProvider,
            IWebHostEnvironment hostingEnv)
        {
            _swaggerGenOptions = swaggerGenOptionsAccessor.Value;
            _serviceProvider = serviceProvider;
            _hostingEnv = hostingEnv;
        }

        public void Configure(SwaggerGeneratorOptions options)
        {
            DeepCopy(_swaggerGenOptions.SwaggerGeneratorOptions, options);

            // Create and add any filters that were specified through the FilterDescriptor lists ...

            _swaggerGenOptions.ParameterFilterDescriptors.ForEach(
                filterDescriptor => options.ParameterFilters.Add(GetOrCreateFilter<IParameterFilter>(filterDescriptor)));

            _swaggerGenOptions.RequestBodyFilterDescriptors.ForEach(
                filterDescriptor => options.RequestBodyFilters.Add(GetOrCreateFilter<IRequestBodyFilter>(filterDescriptor)));

            _swaggerGenOptions.OperationFilterDescriptors.ForEach(
                filterDescriptor => options.OperationFilters.Add(GetOrCreateFilter<IOperationFilter>(filterDescriptor)));

            _swaggerGenOptions.DocumentFilterDescriptors.ForEach(
                filterDescriptor => options.DocumentFilters.Add(GetOrCreateFilter<IDocumentFilter>(filterDescriptor)));


            _swaggerGenOptions.ParameterAsyncFilterDescriptors.ForEach(
                filterDescriptor => options.ParameterAsyncFilters.Add(GetOrCreateFilter<IParameterAsyncFilter>(filterDescriptor)));

            _swaggerGenOptions.RequestBodyAsyncFilterDescriptors.ForEach(
                filterDescriptor => options.RequestBodyAsyncFilters.Add(GetOrCreateFilter<IRequestBodyAsyncFilter>(filterDescriptor)));

            _swaggerGenOptions.OperationAsyncFilterDescriptors.ForEach(
                filterDescriptor => options.OperationAsyncFilters.Add(GetOrCreateFilter<IOperationAsyncFilter>(filterDescriptor)));

            _swaggerGenOptions.DocumentAsyncFilterDescriptors.ForEach(
                filterDescriptor => options.DocumentAsyncFilters.Add(GetOrCreateFilter<IDocumentAsyncFilter>(filterDescriptor)));

            if (!options.SwaggerDocs.Any())
            {
                options.SwaggerDocs.Add("v1", new OpenApiInfo { Title = _hostingEnv.ApplicationName, Version = "1.0" });
            }
        }

        public void DeepCopy(SwaggerGeneratorOptions source, SwaggerGeneratorOptions target)
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
            target.Servers = new List<OpenApiServer>(source.Servers);
            target.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(source.SecuritySchemes);
            target.SecurityRequirements = new List<OpenApiSecurityRequirement>(source.SecurityRequirements);
            target.ParameterFilters = new List<IParameterFilter>(source.ParameterFilters);
            target.ParameterAsyncFilters = new List<IParameterAsyncFilter>(source.ParameterAsyncFilters);
            target.OperationFilters = new List<IOperationFilter>(source.OperationFilters);
            target.OperationAsyncFilters = new List<IOperationAsyncFilter>(source.OperationAsyncFilters);
            target.DocumentFilters = new List<IDocumentFilter>(source.DocumentFilters);
            target.DocumentAsyncFilters = new List<IDocumentAsyncFilter>(source.DocumentAsyncFilters);
            target.RequestBodyFilters = new List<IRequestBodyFilter>(source.RequestBodyFilters);
            target.RequestBodyAsyncFilters = new List<IRequestBodyAsyncFilter>(source.RequestBodyAsyncFilters);
            target.SecuritySchemesSelector = source.SecuritySchemesSelector;
        }

        private TFilter GetOrCreateFilter<TFilter>(FilterDescriptor filterDescriptor)
        {
            return (TFilter)(filterDescriptor.FilterInstance
                ?? ActivatorUtilities.CreateInstance(_serviceProvider, filterDescriptor.Type, filterDescriptor.Arguments));
        }
    }
}
