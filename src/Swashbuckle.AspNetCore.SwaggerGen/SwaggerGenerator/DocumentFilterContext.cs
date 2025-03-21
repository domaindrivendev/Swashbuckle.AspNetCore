using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class DocumentFilterContext
{
    public DocumentFilterContext(
        IEnumerable<ApiDescription> apiDescriptions,
        ISchemaGenerator schemaGenerator,
        SchemaRepository schemaRepository)
    {
        ApiDescriptions = apiDescriptions;
        SchemaGenerator = schemaGenerator;
        SchemaRepository = schemaRepository;
    }

    public IEnumerable<ApiDescription> ApiDescriptions { get; }

    public ISchemaGenerator SchemaGenerator { get; }

    public SchemaRepository SchemaRepository { get; }

    public string DocumentName => SchemaRepository.DocumentName;
}
