using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Swashbuckle.Swagger.Application
{
    public class SwaggerSerializerFactory
    {
        internal static JsonSerializer Create(IOptions<MvcJsonOptions> mvcJsonOptions)
        {
            // TODO: Should this handle case where mvcJsonOptions.Value == null?
            return new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SwaggerContractResolver(mvcJsonOptions.Value.SerializerSettings)
            };
        }
    }
}
