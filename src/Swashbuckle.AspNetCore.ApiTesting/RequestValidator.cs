using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class RequestValidator(IEnumerable<IContentValidator> contentValidators)
{
    private readonly IEnumerable<IContentValidator> _contentValidators = contentValidators;

    public void Validate(
        HttpRequestMessage request,
        OpenApiDocument openApiDocument,
        string pathTemplate,
        HttpMethod operationType)
    {
        var operationSpec = openApiDocument.GetOperationByPathAndType(pathTemplate, operationType, out var pathSpec);
        IOpenApiParameter[] parameterSpecs = [];

        if (pathSpec.Parameters is { Count: > 0 } pathParameters)
        {
            parameterSpecs = [.. parameterSpecs, .. pathParameters];
        }

        if (operationSpec.Parameters is { Count: > 0 } operationParameters)
        {
            parameterSpecs = [.. parameterSpecs, .. operationParameters];
        }

        // Convert to absolute Uri as a workaround to limitation with Uri class - i.e. most of it's methods are not supported for relative Uri's.
        var requestUri = new Uri(new Uri("http://tempuri.org"), request.RequestUri);

        if (!TryParsePathNameValues(pathTemplate, requestUri.AbsolutePath, out NameValueCollection pathNameValues))
        {
            throw new RequestDoesNotMatchSpecException($"Request URI '{requestUri.AbsolutePath}' does not match specified template '{pathTemplate}'");
        }

        if (request.Method != new HttpMethod(operationType.ToString()))
        {
            throw new RequestDoesNotMatchSpecException($"Request method '{request.Method}' does not match specified operation type '{operationType}'");
        }

        ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Path), pathNameValues);
        ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Query), HttpUtility.ParseQueryString(requestUri.Query));
        ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Header), request.Headers.ToNameValueCollection());

        if (operationSpec.RequestBody != null)
        {
            ValidateContent(operationSpec.RequestBody, openApiDocument, request.Content);
        }
    }

    private static bool TryParsePathNameValues(string pathTemplate, string requestUri, out NameValueCollection pathNameValues)
    {
        pathNameValues = [];

        var templateMatcher = new TemplateMatcher(TemplateParser.Parse(pathTemplate), null);
        var routeValues = new RouteValueDictionary();
        if (!templateMatcher.TryMatch(new PathString(requestUri), routeValues))
        {
            return false;
        }

        foreach (var entry in routeValues)
        {
            pathNameValues.Add(entry.Key, entry.Value.ToString());
        }

        return true;
    }


    private static void ValidateParameters(
        IEnumerable<IOpenApiParameter> parameterSpecs,
        NameValueCollection parameterNameValues)
    {
        foreach (var parameterSpec in parameterSpecs)
        {
            var value = parameterNameValues[parameterSpec.Name];

            if ((parameterSpec.In == ParameterLocation.Path || parameterSpec.Required) && value == null)
            {
                throw new RequestDoesNotMatchSpecException($"Required parameter '{parameterSpec.Name}' is not present");
            }

            if (value == null || parameterSpec.Schema == null)
            {
                continue;
            }

            if (parameterSpec.Schema is OpenApiSchema schema && !schema.TryParse(value, out object typedValue))
            {
                throw new RequestDoesNotMatchSpecException($"Parameter '{parameterSpec.Name}' is not of type '{schema.TypeIdentifier()}'");
            }
        }
    }

    private void ValidateContent(IOpenApiRequestBody requestBodySpec, OpenApiDocument openApiDocument, HttpContent content)
    {
        if (requestBodySpec.Required && content == null)
        {
            throw new RequestDoesNotMatchSpecException("Required content is not present");
        }

        if (content == null)
        {
            return;
        }

        if (!requestBodySpec.Content.TryGetValue(content.Headers.ContentType.MediaType, out OpenApiMediaType mediaTypeSpec))
        {
            throw new RequestDoesNotMatchSpecException($"Content media type '{content.Headers.ContentType.MediaType}' is not specified");
        }

        try
        {
            foreach (var contentValidator in _contentValidators)
            {
                if (contentValidator.CanValidate(content.Headers.ContentType.MediaType))
                {
                    contentValidator.Validate(mediaTypeSpec, openApiDocument, content);
                }
            }
        }
        catch (ContentDoesNotMatchSpecException contentException)
        {
            throw new RequestDoesNotMatchSpecException($"Content does not match spec. {contentException.Message}");
        }
    }
}
