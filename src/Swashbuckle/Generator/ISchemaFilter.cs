using System;

namespace Swashbuckle.Swagger.Generator
{
    public interface ISchemaFilter
    {
        void Apply(Schema schema, SchemaGenerator schemaGenerator, Type type);
    }
}