using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class DocumentFilterContext(
    IEnumerable<ApiDescription> apiDescriptions,
    ISchemaGenerator schemaGenerator,
    SchemaRepository schemaRepository)
{
    public IEnumerable<ApiDescription> ApiDescriptions { get; } = apiDescriptions;

    public ISchemaGenerator SchemaGenerator { get; } = schemaGenerator;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public string DocumentName => SchemaRepository.DocumentName;
}
