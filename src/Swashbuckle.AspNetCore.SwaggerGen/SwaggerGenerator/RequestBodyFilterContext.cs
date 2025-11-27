using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class RequestBodyFilterContext(
    ApiParameterDescription bodyParameterDescription,
    IEnumerable<ApiParameterDescription> formParameterDescriptions,
    ISchemaGenerator schemaGenerator,
    SchemaRepository schemaRepository,
    OpenApiDocument document)
{
    public ApiParameterDescription BodyParameterDescription { get; } = bodyParameterDescription;

    public IEnumerable<ApiParameterDescription> FormParameterDescriptions { get; } = formParameterDescriptions;

    public ISchemaGenerator SchemaGenerator { get; } = schemaGenerator;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public OpenApiDocument Document { get; } = document;

    public string DocumentName => SchemaRepository.DocumentName;
}
