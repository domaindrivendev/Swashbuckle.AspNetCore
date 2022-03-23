using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
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
                filterDescriptor => options.ParameterFilters.Add(CreateFilter<IParameterFilter>(filterDescriptor)));

            _swaggerGenOptions.RequestBodyFilterDescriptors.ForEach(
                filterDescriptor => options.RequestBodyFilters.Add(CreateFilter<IRequestBodyFilter>(filterDescriptor)));

            _swaggerGenOptions.OperationFilterDescriptors.ForEach(
                filterDescriptor => options.OperationFilters.Add(CreateFilter<IOperationFilter>(filterDescriptor)));

            _swaggerGenOptions.DocumentFilterDescriptors.ForEach(
                filterDescriptor => options.DocumentFilters.Add(CreateFilter<IDocumentFilter>(filterDescriptor)));

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
            target.DescribeAllParametersInCamelCase = source.DescribeAllParametersInCamelCase;
            target.Servers = new List<OpenApiServer>(source.Servers);
            target.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(source.SecuritySchemes);
            target.SecurityRequirements = new List<OpenApiSecurityRequirement>(source.SecurityRequirements);
            target.ParameterFilters = new List<IParameterFilter>(source.ParameterFilters);
            target.OperationFilters = new List<IOperationFilter>(source.OperationFilters);
            target.DocumentFilters = new List<IDocumentFilter>(source.DocumentFilters);
        }

        private TFilter CreateFilter<TFilter>(FilterDescriptor filterDescriptor)
        {
            return (TFilter)ActivatorUtilities
                .CreateInstance(_serviceProvider, filterDescriptor.Type, filterDescriptor.Arguments);
        }
    }
}