using System;
using System.Collections.Generic;

namespace Swashbuckle.Swagger
{
    public class SchemaGeneratorOptions
    {
        public SchemaGeneratorOptions()
        {
            CustomTypeMappings = new Dictionary<Type, Func<Schema>>();
            ModelFilters = new List<IModelFilter>();
        }

        public IDictionary<Type, Func<Schema>> CustomTypeMappings { get; private set; }

        public IList<IModelFilter> ModelFilters { get; private set; }

        public bool IgnoreObsoleteProperties { get; set; }

        public bool UseFullTypeNameInSchemaIds { get; set; }

        public bool DescribeAllEnumsAsStrings { get; set; }

        public bool DescribeStringEnumsInCamelCase { get; set; }
    }
}