using Swashbuckle.SwaggerGen.Extensions;
using System;
using System.Collections.Generic;

namespace Swashbuckle.SwaggerGen.Generator
{
    public class SchemaRegistryOptions
    {
        public SchemaRegistryOptions()
        {
            CustomTypeMappings = new Dictionary<Type, Func<Schema>>();
            SchemaIdSelector = (type) => type.FriendlyId(false);
            ModelFilters = new List<IModelFilter>();
        }

        public IDictionary<Type, Func<Schema>> CustomTypeMappings { get; private set; }

        public bool DescribeAllEnumsAsStrings { get; set; }

        public bool DescribeAllEnumsAsIdTextPair { get; set; }

        public bool DescribeStringEnumsInCamelCase { get; set; }

        public Func<Type, string> SchemaIdSelector { get; set; }

        public bool IgnoreObsoleteProperties { get; set; }

        public IList<IModelFilter> ModelFilters { get; private set; }

        internal SchemaRegistryOptions Clone()
        {
            return new SchemaRegistryOptions
            {
                CustomTypeMappings = CustomTypeMappings,
                DescribeAllEnumsAsStrings = DescribeAllEnumsAsStrings,
                DescribeAllEnumsAsIdTextPair = DescribeAllEnumsAsIdTextPair,
                DescribeStringEnumsInCamelCase = DescribeStringEnumsInCamelCase,
                IgnoreObsoleteProperties = IgnoreObsoleteProperties,
                SchemaIdSelector = SchemaIdSelector,
                ModelFilters = ModelFilters
            };
        }
    }
}