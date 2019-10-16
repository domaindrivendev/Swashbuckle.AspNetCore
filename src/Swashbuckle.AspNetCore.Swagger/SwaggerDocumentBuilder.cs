using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerDocumentBuilder
    {
        private readonly SwaggerOptions _options;

        public SwaggerDocumentBuilder(SwaggerOptions options)
        {
            _options = options ?? new SwaggerOptions();
        }

        public string Build(
            HttpRequest httpRequest,
            ISwaggerProvider swaggerProvider,
            string documentName)
        {
            var basePath = string.IsNullOrEmpty(httpRequest.PathBase)
                ? null
                : httpRequest.PathBase.ToString();

            var swagger =
                swaggerProvider.GetSwagger(documentName, null, basePath);

            // One last opportunity to modify the Swagger Document - this time with request context
            foreach (var filter in _options.PreSerializeFilters)
            {
                filter(swagger, httpRequest);
            }

            using (var textWriter = new StringWriter())
            {
                var jsonWriter = new OpenApiJsonWriter(textWriter);

                if (_options.SerializeAsV2)
                {
                    swagger.SerializeAsV2(jsonWriter);
                }
                else
                {
                    swagger.SerializeAsV3(jsonWriter);
                }

                return textWriter.ToString();
            }
        }
    }
}
