using System;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Application
{
    public class SwaggerDocsContractResolver : DefaultContractResolver
    {
        private readonly CamelCasePropertyNamesContractResolver _camelCasePropertyNamesContractResolver;

        public SwaggerDocsContractResolver()
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