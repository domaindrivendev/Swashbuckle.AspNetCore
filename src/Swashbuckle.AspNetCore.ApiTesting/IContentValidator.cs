using Microsoft.OpenApi.Models;
using System;
using System.Net.Http;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public interface IContentValidator
    {
        bool CanValidate(string mediaType);

        void Validate(OpenApiMediaType mediaTypeSpec, OpenApiDocument openApiDocument, HttpContent content);
    }

    public class ContentDoesNotMatchSpecException : Exception
    {
        public ContentDoesNotMatchSpecException(string message)
            : base(message)
        { }
    }
}
