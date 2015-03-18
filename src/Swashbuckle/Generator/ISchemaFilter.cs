using System;

namespace Swashbuckle.Swagger
{
    public interface ISchemaFilter
    {
        void Apply(Schema schema, SchemaGenerator schemaGenerator, Type type);
    }
}