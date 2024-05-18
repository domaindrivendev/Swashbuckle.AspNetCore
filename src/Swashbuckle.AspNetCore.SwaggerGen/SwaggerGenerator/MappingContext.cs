using System;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface IMappingContext
{
    ISchemaGenerator SchemaGenerator { get; }
    SchemaRepository SchemaRepository { get; }

    /// <summary>
    /// Actual runtime type that's being mapped.
    /// </summary>
    Type UnderlyingType { get;}
}

public class MappingContext(
    ISchemaGenerator schemaGenerator,
    SchemaRepository schemaRepository,
    Type underlyingType)
    : IMappingContext
{
    public ISchemaGenerator SchemaGenerator { get; } = schemaGenerator;
    public SchemaRepository SchemaRepository { get; } = schemaRepository;
    public Type UnderlyingType { get; } = underlyingType;
}
