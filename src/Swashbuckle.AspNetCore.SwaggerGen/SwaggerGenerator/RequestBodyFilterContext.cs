using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class RequestBodyFilterContext
{
    public RequestBodyFilterContext(
        ApiParameterDescription bodyParameterDescription,
        IEnumerable<ApiParameterDescription> formParameterDescriptions,
        ISchemaGenerator schemaGenerator,
        SchemaRepository schemaRepository)
    {
        BodyParameterDescription = bodyParameterDescription;
        FormParameterDescriptions = formParameterDescriptions;
        SchemaGenerator = schemaGenerator;
        SchemaRepository = schemaRepository;
    }

    public ApiParameterDescription BodyParameterDescription { get; }

    public IEnumerable<ApiParameterDescription> FormParameterDescriptions { get; }

    public ISchemaGenerator SchemaGenerator { get; }

    public SchemaRepository SchemaRepository { get; }

    public string DocumentName => SchemaRepository.DocumentName;
}
