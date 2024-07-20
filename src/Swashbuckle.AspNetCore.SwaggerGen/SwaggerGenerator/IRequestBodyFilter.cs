using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IRequestBodyFilter
    {
        void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context);
    }

    public interface IRequestBodyAsyncFilter
    {
        Task ApplyAsync(OpenApiRequestBody requestBody, RequestBodyFilterContext context, CancellationToken cancellationToken);
    }

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
}
