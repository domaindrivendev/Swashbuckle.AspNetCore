using System;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Application
{
    public class SwaggerGenContractResolver : DefaultContractResolver
    {
        private readonly CamelCasePropertyNamesContractResolver _camelCasePropertyNamesContractResolver;

        public SwaggerGenContractResolver()
        {
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