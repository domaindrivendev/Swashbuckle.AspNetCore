using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class MappingContext
    {
        public MappingContext(
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository,
            Type underlyingType)
        {
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
            UnderlyingType = underlyingType;
        }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public Type UnderlyingType { get; }
    }
}
