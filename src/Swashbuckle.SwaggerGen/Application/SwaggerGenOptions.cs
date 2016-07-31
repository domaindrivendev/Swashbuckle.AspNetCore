using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.Annotations;

namespace Swashbuckle.SwaggerGen.Application
{
    public class SwaggerGenOptions
    {
        private readonly SwaggerGeneratorOptions _swaggerGeneratorOptions;
        private readonly SchemaRegistryOptions _schemaRegistryOptions;

        private List<FilterDescriptor<IOperationFilter>> _operationFilterDescriptors;
        private List<FilterDescriptor<IDocumentFilter>> _documentFilterDescriptors;
        private List<FilterDescriptor<ISchemaFilter>> _schemaFilterDescriptors;

        private struct FilterDescriptor<TFilter>
        {
            public Type Type;
            public object[] Arguments;
        }

        public SwaggerGenOptions()
        {
            _swaggerGeneratorOptions = new SwaggerGeneratorOptions();
            _schemaRegistryOptions = new SchemaRegistryOptions();

            _operationFilterDescriptors = new List<FilterDescriptor<IOperationFilter>>();
            _documentFilterDescriptors = new List<FilterDescriptor<IDocumentFilter>>();
            _schemaFilterDescriptors = new List<FilterDescriptor<ISchemaFilter>>();

            // Enable Annotations
            OperationFilter<SwaggerAttributesOperationFilter>();
            SchemaFilter<SwaggerAttributesSchemaFilter>();
        }

        public void SingleApiVersion(Info info)
        {
            _swaggerGeneratorOptions.SingleApiVersion(info);
        }

        public void MultipleApiVersions(
            IEnumerable<Info> apiVersions,
            Func<ApiDescription, string, bool> versionSupportResolver)
        {
            _swaggerGeneratorOptions.MultipleApiVersions(apiVersions, versionSupportResolver);
        }

        public void IgnoreObsoleteActions()
        {
            _swaggerGeneratorOptions.IgnoreObsoleteActions = true;
        }

        public void GroupActionsBy(Func<ApiDescription, string> groupNameSelector)
        {
            _swaggerGeneratorOptions.GroupNameSelector = groupNameSelector;
        }

        public void OrderActionGroupsBy(IComparer<string> groupNameComparer)
        {
            _swaggerGeneratorOptions.GroupNameComparer = groupNameComparer;
        }

        public void AddSecurityDefinition(string name, SecurityScheme securityScheme)
        {
            _swaggerGeneratorOptions.SecurityDefinitions.Add(name, securityScheme);
        }

        public void MapType(Type type, Func<Schema> schemaFactory)
        {
            _schemaRegistryOptions.CustomTypeMappings.Add(type, schemaFactory);
        }

        public void MapType<T>(Func<Schema> schemaFactory)
        {
            MapType(typeof(T), schemaFactory);
        }

        public void DescribeAllEnumsAsStrings()
        {
            _schemaRegistryOptions.DescribeAllEnumsAsStrings = true;
        }

        public void DescribeStringEnumsInCamelCase()
        {
            _schemaRegistryOptions.DescribeStringEnumsInCamelCase = true;
        }

        public void CustomSchemaIds(Func<Type, string> schemaIdSelector)
        {
            _schemaRegistryOptions.SchemaIdSelector = schemaIdSelector; 
        }

        public void IgnoreObsoleteProperties()
        {
            _schemaRegistryOptions.IgnoreObsoleteProperties = true;
        }

        public void OperationFilter<TFilter>(params object[] parameters)
            where TFilter : IOperationFilter
        {
            _operationFilterDescriptors.Add(new FilterDescriptor<IOperationFilter>
            {
                Type = typeof(TFilter),
                Arguments = parameters
            });
        }

        public void DocumentFilter<TFilter>(params object[] parameters)
            where TFilter : IDocumentFilter
        {
            _documentFilterDescriptors.Add(new FilterDescriptor<IDocumentFilter>
            {
                Type = typeof(TFilter),
                Arguments = parameters
            });
        }

        public void SchemaFilter<TFilter>(params object[] parameters)
            where TFilter : ISchemaFilter
        {
            _schemaFilterDescriptors.Add(new FilterDescriptor<ISchemaFilter>
            {
                Type = typeof(TFilter),
                Arguments = parameters
            });
        }

        public void IncludeXmlComments(string xmlDocPath)
        {
            var xmlDoc = new XPathDocument(xmlDocPath);
            OperationFilter<XmlCommentsOperationFilter>(xmlDoc);
            SchemaFilter<XmlCommentsSchemaFilter>(xmlDoc);
        }

        internal SchemaRegistryOptions GetSchemaRegistryOptions(IServiceProvider serviceProvider)
        {
            var options = _schemaRegistryOptions.Clone();

            foreach (var filter in CreateFilters(_schemaFilterDescriptors, serviceProvider))
            {
                options.SchemaFilters.Add(filter);
            }

            return options;
        }

        internal SwaggerGeneratorOptions GetSwaggerGeneratorOptions(IServiceProvider serviceProvider)
        {
            var options = _swaggerGeneratorOptions.Clone();

            foreach (var filter in CreateFilters(_operationFilterDescriptors, serviceProvider))
            {
                options.OperationFilters.Add(filter);
            }

            foreach (var filter in CreateFilters(_documentFilterDescriptors, serviceProvider))
            {
                options.DocumentFilters.Add(filter);
            }

            return options;
        }

        private IEnumerable<TFilter> CreateFilters<TFilter>(
            List<FilterDescriptor<TFilter>> _filterDescriptors,
            IServiceProvider serviceProvider)
        {
            return _filterDescriptors.Select(descriptor =>
            {
                return (TFilter)ActivatorUtilities.CreateInstance(serviceProvider, descriptor.Type, descriptor.Arguments);
            });
        }
    }
}
