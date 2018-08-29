using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ConfigureSwaggerGeneratorOptions : IConfigureOptions<SwaggerGeneratorOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SwaggerGenOptions _swaggerGenOptions;

        public ConfigureSwaggerGeneratorOptions(
            IServiceProvider serviceProvider,
            IOptions<SwaggerGenOptions> swaggerGenOptionsAccessor)
        {
            _serviceProvider = serviceProvider;
            _swaggerGenOptions = swaggerGenOptionsAccessor.Value;
        }

        public void Configure(SwaggerGeneratorOptions options)
        {
            DeepCopy(_swaggerGenOptions.SwaggerGeneratorOptions, options);

            // Create and add any filters that were specified through the FilterDescriptor lists ...

            _swaggerGenOptions.ParameterFilterDescriptors.ForEach(
                filterDescriptor => options.ParameterFilters.Add(CreateFilter<IParameterFilter>(filterDescriptor)));

            _swaggerGenOptions.OperationFilterDescriptors.ForEach(
                filterDescriptor => options.OperationFilters.Add(CreateFilter<IOperationFilter>(filterDescriptor)));

            _swaggerGenOptions.DocumentFilterDescriptors.ForEach(
                filterDescriptor => options.DocumentFilters.Add(CreateFilter<IDocumentFilter>(filterDescriptor)));
        }

        public void DeepCopy(SwaggerGeneratorOptions source, SwaggerGeneratorOptions target)
        {
            target.SwaggerDocs = new Dictionary<string, Info>(source.SwaggerDocs);
            target.DocInclusionPredicate = source.DocInclusionPredicate;
            target.IgnoreObsoleteActions = source.IgnoreObsoleteActions;
            target.ConflictingActionsResolver = source.ConflictingActionsResolver;
            target.TagSelector = source.TagSelector;
            target.SortKeySelector = source.SortKeySelector;
            target.DescribeAllParametersInCamelCase = source.DescribeAllParametersInCamelCase;
            target.SecurityDefinitions = new Dictionary<string, SecurityScheme>(source.SecurityDefinitions);
            target.SecurityRequirements = new List<IDictionary<string, IEnumerable<string>>>(source.SecurityRequirements);
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