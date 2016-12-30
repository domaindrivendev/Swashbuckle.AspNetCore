using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerSerializerFactory
    {
        internal static JsonSerializer Create(IOptions<MvcJsonOptions> mvcJsonOptions)
        {
            // TODO: Should this handle case where mvcJsonOptions.Value == null?
            return new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = mvcJsonOptions.Value.SerializerSettings.Formatting,
                ContractResolver = new SwaggerContractResolver(mvcJsonOptions.Value.SerializerSettings)
            };
        }
    }
}
