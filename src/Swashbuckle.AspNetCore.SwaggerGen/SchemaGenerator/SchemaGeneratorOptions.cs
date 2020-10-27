using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGeneratorOptions
    {
        public SchemaGeneratorOptions()
        {
            SchemaIdSelector = DefaultSchemaIdSelector;
            SubTypesSelector = DefaultSubTypesSelector;
            SchemaFilters = new List<ISchemaFilter>();
            SchemaMappingProviders = new List<ISchemaMappingProvider>();
            #pragma warning disable 0612, 0618
            CustomTypeMappings = new LegacyTypeMappingCollection(this);
            #pragma warning restore 0612, 0618
        }

        [Obsolete("Use SchemaMappings")]
        public LegacyTypeMappingCollection CustomTypeMappings { get; }

        public IList<ISchemaMappingProvider> SchemaMappingProviders { get; set; }

        public bool UseInlineDefinitionsForEnums { get; set; }

        public Func<Type, string> SchemaIdSelector { get; set; }

        public bool IgnoreObsoleteProperties { get; set; }

        public bool UseAllOfToExtendReferenceSchemas { get; set; }

        public bool UseAllOfForInheritance { get; set; }

        public bool UseOneOfForPolymorphism { get; set; }

        public Func<Type, IEnumerable<Type>> SubTypesSelector { get; set; }

        public Func<Type, string> DiscriminatorNameSelector { get; set; }

        public Func<Type, string> DiscriminatorValueSelector { get; set; }

        public IList<ISchemaFilter> SchemaFilters { get; set; }

        private string DefaultSchemaIdSelector(Type modelType)
        {
            if (!modelType.IsConstructedGenericType) return modelType.Name.Replace("[]", "Array");

            var prefix = modelType.GetGenericArguments()
                .Select(genericArg => DefaultSchemaIdSelector(genericArg))
                .Aggregate((previous, current) => previous + current);

            return prefix + modelType.Name.Split('`').First();
        }

        private IEnumerable<Type> DefaultSubTypesSelector(Type baseType)
        {
            return baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
        }
    }
}