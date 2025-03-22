using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class RequestBodyFilterContext(
    ApiParameterDescription bodyParameterDescription,
    IEnumerable<ApiParameterDescription> formParameterDescriptions,
    ISchemaGenerator schemaGenerator,
    SchemaRepository schemaRepository)
{
    public ApiParameterDescription BodyParameterDescription { get; } = bodyParameterDescription;

    public IEnumerable<ApiParameterDescription> FormParameterDescriptions { get; } = formParameterDescriptions;

    public ISchemaGenerator SchemaGenerator { get; } = schemaGenerator;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public string DocumentName => SchemaRepository.DocumentName;
}
