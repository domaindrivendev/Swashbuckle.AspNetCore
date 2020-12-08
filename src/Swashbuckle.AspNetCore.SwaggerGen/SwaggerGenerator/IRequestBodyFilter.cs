using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IRequestBodyFilter
    {
        void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context);
    }

    public class RequestBodyFilterContext
    {
        public RequestBodyFilterContext(
            ApiParameterDescription bodyParameterDescription,
            IEnumerable<ApiParameterDescription> formParameterDescriptions,
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository,
            string documentName = null)
        {
            BodyParameterDescription = bodyParameterDescription;
            FormParameterDescriptions = formParameterDescriptions;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
            DocumentName = documentName;
        }

        public ApiParameterDescription BodyParameterDescription { get; }

        public IEnumerable<ApiParameterDescription> FormParameterDescriptions { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public string DocumentName { get; }
    }
}
