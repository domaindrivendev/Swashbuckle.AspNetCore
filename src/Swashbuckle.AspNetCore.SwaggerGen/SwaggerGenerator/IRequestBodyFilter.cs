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
            SchemaRepository schemaRepository)
        {
            BodyParameterDescription = bodyParameterDescription;
            FormParameterDescriptions = formParameterDescriptions;
            SchemaRepository = schemaRepository;
        }

        public ApiParameterDescription BodyParameterDescription { get; }

        public IEnumerable<ApiParameterDescription> FormParameterDescriptions { get; }

        public SchemaRepository SchemaRepository { get; }
    }
}
