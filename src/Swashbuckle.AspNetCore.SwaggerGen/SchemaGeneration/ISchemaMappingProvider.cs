namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaMappingProvider
    {
        bool CanCreateMapping(SchemaMappingContext context);

        SchemaMapping CreateMapping(SchemaMappingContext context);
    }
}
