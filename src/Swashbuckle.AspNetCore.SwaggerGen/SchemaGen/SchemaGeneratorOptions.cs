using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGeneratorOptions
    {
        public SchemaGeneratorOptions()
        {
            CustomTypeMappings = new Dictionary<Type, Func<OpenApiSchema>>();
            SchemaIdSelector = DefaultSchemaIdSelector;
            SubTypesResolver = DefaultSubTypeResolver;
            SchemaFilters = new List<ISchemaFilter>();
        }

        public IDictionary<Type, Func<OpenApiSchema>> CustomTypeMappings { get; set; }

        public bool DescribeAllEnumsAsStrings { get; set; }

        public bool DescribeStringEnumsInCamelCase { get; set; }

        public bool UseReferencedDefinitionsForEnums { get; set; }

        public Func<Type, string> SchemaIdSelector { get; set; }

        public bool IgnoreObsoleteProperties { get; set; }

        public bool GeneratePolymorphicSchemas { get; set; }

        public Func<Type, IEnumerable<Type>> SubTypesResolver { get; set; }

        public IList<ISchemaFilter> SchemaFilters { get; set; }

        private string DefaultSchemaIdSelector(Type modelType)
        {
            if (!modelType.IsConstructedGenericType) return modelType.Name;

            var prefix = modelType.GetGenericArguments()
                .Select(genericArg => DefaultSchemaIdSelector(genericArg))
                .Aggregate((previous, current) => previous + current);

            return prefix + modelType.Name.Split('`').First();
        }

        private IEnumerable<Type> DefaultSubTypeResolver(Type baseType)
        {
            return baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
        }
    }
}