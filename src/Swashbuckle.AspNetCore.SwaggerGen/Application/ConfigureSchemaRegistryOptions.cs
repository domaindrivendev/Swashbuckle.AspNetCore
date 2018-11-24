using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ConfigureSchemaRegistryOptions : IConfigureOptions<SchemaRegistryOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SwaggerGenOptions _swaggerGenOptions;

        public ConfigureSchemaRegistryOptions(
            IServiceProvider serviceProvider,
            IOptions<SwaggerGenOptions> swaggerGenOptionsAccessor)
        {
            _serviceProvider = serviceProvider;
            _swaggerGenOptions = swaggerGenOptionsAccessor.Value;
        }

        public void Configure(SchemaRegistryOptions options)
        {
            DeepCopy(_swaggerGenOptions.SchemaRegistryOptions, options);

            // Create and add any filters that were specified through the FilterDescriptor lists
            _swaggerGenOptions.SchemaFilterDescriptors.ForEach(
                filterDescriptor => options.SchemaFilters.Add(CreateFilter<ISchemaFilter>(filterDescriptor)));
        }

        private void DeepCopy(SchemaRegistryOptions source, SchemaRegistryOptions target)
        {
            target.CustomTypeMappings = new Dictionary<Type, Func<OpenApiSchema>>(source.CustomTypeMappings);
            target.DescribeAllEnumsAsStrings = source.DescribeAllEnumsAsStrings;
            target.DescribeStringEnumsInCamelCase = source.DescribeStringEnumsInCamelCase;
            target.UseReferencedDefinitionsForEnums = source.UseReferencedDefinitionsForEnums;
            target.SchemaIdSelector = source.SchemaIdSelector;
            target.IgnoreObsoleteProperties = source.IgnoreObsoleteProperties;
            target.SchemaFilters = new List<ISchemaFilter>(source.SchemaFilters);
        }

        private TFilter CreateFilter<TFilter>(FilterDescriptor filterDescriptor)
        {
            return (TFilter)ActivatorUtilities
                .CreateInstance(_serviceProvider, filterDescriptor.Type, filterDescriptor.Arguments);
        }
    }
}