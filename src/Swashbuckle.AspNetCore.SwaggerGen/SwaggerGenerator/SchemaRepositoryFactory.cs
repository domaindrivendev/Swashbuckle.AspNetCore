using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRepositoryFactory
    {
        private readonly IEnumerable<ISchemaMappingProvider> _schemaMappingProviders;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SchemaGeneratorOptions _schemaGeneratorOptions;

        public SchemaRepositoryFactory(IEnumerable<ISchemaMappingProvider> schemaMappingProviders, ISchemaGenerator schemaGenerator, SchemaGeneratorOptions schemaGeneratorOptions) {
            _schemaMappingProviders = schemaMappingProviders.ToArray();
            _schemaGenerator = schemaGenerator;
            _schemaGeneratorOptions = schemaGeneratorOptions;
        }

        public SchemaRepository Create() => new SchemaRepository(
            schemaMappingProviders: _schemaMappingProviders,
            schemaGenerator: _schemaGenerator,
            generatorOptions: _schemaGeneratorOptions
        );
    }
}