using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger.Application
{
    public class SwaggerContractResolver : DefaultContractResolver
    {
        private readonly JsonSerializer _appJsonSerializer;
        private readonly CamelCasePropertyNamesContractResolver _camelCasePropertyNamesContractResolver;

        public SwaggerContractResolver(JsonSerializerSettings appSerializerSettings)
        {
            _appJsonSerializer = JsonSerializer.Create(appSerializerSettings);
            _camelCasePropertyNamesContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public override JsonContract ResolveContract(Type type)
        {
            var defaultContract = base.ResolveContract(type);
            if (defaultContract is JsonDictionaryContract) return defaultContract;

            return _camelCasePropertyNamesContractResolver.ResolveContract(type);
        }
    }
}