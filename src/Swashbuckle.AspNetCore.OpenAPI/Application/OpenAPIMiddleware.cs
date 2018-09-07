using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Validations;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.OpenAPI.Application
{
    public class OpenAPIMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializer _swaggerSerializer;
        private readonly SwaggerOptions _options;
        private readonly TemplateMatcher _requestMatcher;
        private readonly OpenAPIOptions _optionsOpenAPI;

        public OpenAPIMiddleware(
            RequestDelegate next,
            IOptions<MvcJsonOptions> mvcJsonOptionsAccessor,
            IOptions<SwaggerOptions> optionsAccessor,
            IOptions<OpenAPIOptions> openApiOptionsAcessor)
            : this(next, mvcJsonOptionsAccessor, optionsAccessor.Value, openApiOptionsAcessor.Value)
        { }

        public OpenAPIMiddleware(
            RequestDelegate next,
            IOptions<MvcJsonOptions> mvcJsonOptions,
            SwaggerOptions options,
            OpenAPIOptions openAPIOptions)
        {
            _next = next;
            _swaggerSerializer = SwaggerSerializerFactory.Create(mvcJsonOptions);
            _options = options ?? new SwaggerOptions();
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(options.RouteTemplate), new RouteValueDictionary());
            _optionsOpenAPI = openAPIOptions ?? new OpenAPIOptions();
        }

        public async Task Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
        {
            if (!RequestingSwaggerDocument(httpContext.Request, out string documentName))
            {
                await _next(httpContext);
                return;
            }

            var basePath = string.IsNullOrEmpty(httpContext.Request.PathBase)
                ? null
                : httpContext.Request.PathBase.ToString();

            try
            {
                SwaggerDocument swagger;

                //if it version 2.0 schemes are not mandatory
                //but to use the Microsoft OpenAPI.NET we need at least one scheme
                if (_optionsOpenAPI.CompabilityLayerActive &&
                    _optionsOpenAPI.Version == OpenApiSpecVersion.OpenApi2_0)
                {
                    swagger = swaggerProvider.GetSwagger(documentName, null, basePath, httpContext.Request.IsHttps ? new string[] {"https"} : new string []{ "http" });
                }
                else
                {
                    swagger = swaggerProvider.GetSwagger(documentName, null, basePath);
                }

                // One last opportunity to modify the Swagger Document - this time with request context
                foreach (var filter in _options.PreSerializeFilters)
                {
                    filter(swagger, httpContext.Request);
                }

                if(_optionsOpenAPI.CompabilityLayerActive)
                {
                    await RespondWithOpenApiJson(httpContext.Response, swagger);
                }
                else
                {
                    await RespondWithSwaggerJson(httpContext.Response, swagger);
                }
            }
            catch (UnknownSwaggerDocument)
            {
                RespondWithNotFound(httpContext.Response);
            }
        }

        private bool RequestingSwaggerDocument(HttpRequest request, out string documentName)
        {
            documentName = null;
            if (request.Method != "GET") return false;

            var routeValues = new RouteValueDictionary();
            if (!_requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.ContainsKey("documentName")) return false;

            documentName = routeValues["documentName"].ToString();
            return true;
        }

        private void RespondWithNotFound(HttpResponse response)
        {
            response.StatusCode = 404;
        }

        private async Task RespondWithSwaggerJson(HttpResponse response, SwaggerDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                _swaggerSerializer.Serialize(writer, swagger);
                await response.WriteAsync(jsonBuilder.ToString(), new UTF8Encoding(false));
            }
        }

        private async Task RespondWithOpenApiJson(HttpResponse response, SwaggerDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            if (_optionsOpenAPI.Format == OpenApiFormat.Yaml)
            {
                response.ContentType = "application/yaml;charset=utf-8";
            }

            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                _swaggerSerializer.Serialize(writer, swagger);

                await response.WriteAsync(ConvertToOpenAPI(jsonBuilder.ToString()), new UTF8Encoding(false));
            }
        }

        private MemoryStream CreateStream(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        private string ConvertToOpenAPI(string text)
        {
            Stream stream = CreateStream(text);

            var document = new OpenApiStreamReader(new OpenApiReaderSettings
            {
                ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
                RuleSet = ValidationRuleSet.GetDefaultRuleSet()
            }
            ).Read(stream, out var context);

            if (context.Errors.Any())
            { 
                var errorReport = new StringBuilder();

                foreach (var error in context.Errors)
                {
                    errorReport.AppendLine(error.ToString());
                }

                throw new ConversionException(errorReport.ToString());
            }

            return WriteContents(document);
        }

        private string WriteContents(OpenApiDocument document)
        {
            var outputStream = new MemoryStream();
            document.Serialize(
                outputStream,
                _optionsOpenAPI.Version,
                _optionsOpenAPI.Format);

            outputStream.Position = 0;

            return new StreamReader(outputStream).ReadToEnd();
        }
    }
}
