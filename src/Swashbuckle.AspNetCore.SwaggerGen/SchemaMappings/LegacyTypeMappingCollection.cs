using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings
{
    [Obsolete]
    public class LegacyTypeMappingCollection
    {
        private readonly SchemaGeneratorOptions _options;

        public LegacyTypeMappingCollection(SchemaGeneratorOptions options) {
            _options = options;
        }

        public void Add(Type type, Func<OpenApiSchema> schemaProvider) {
            _options.SchemaMappingProviders.Add(new LegacySchemaMappingProvider(type, schemaProvider));
        }
    }
}