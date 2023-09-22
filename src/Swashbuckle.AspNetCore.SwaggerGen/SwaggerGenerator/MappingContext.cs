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

        public readonly ISchemaGenerator SchemaGenerator;

        public readonly SchemaRepository SchemaRepository;

        /// <summary>
        /// Actual runtime type that's being mapped.
        /// </summary>
        public readonly Type UnderlyingType;
    }
}
