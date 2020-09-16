using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions.Add("X-foo", new OpenApiString("bar"));
        }
    }

    public class VendorExtensionsAsyncDocumentFilter : IAsyncDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            throw new System.NotImplementedException();
        }

        public async Task ApplyAsync(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            await Task.Delay(1000);
            swaggerDoc.Extensions.Add("X-foo", new OpenApiString("bar"));
        }
    }
}