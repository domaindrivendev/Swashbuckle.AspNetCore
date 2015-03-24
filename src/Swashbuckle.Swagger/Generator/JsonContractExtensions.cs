using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger.Generator
{
    public static class JsonContractExtensions
    {
        public static bool IsSelfReferencing(this JsonDictionaryContract dictionaryContract)
        {
            return dictionaryContract.UnderlyingType == dictionaryContract.DictionaryValueType;
        }

        public static bool IsSelfReferencing(this JsonArrayContract arrayContract)
        {
            return arrayContract.UnderlyingType == arrayContract.CollectionItemType;
        }

        public static bool IsDeterministic(this JsonContract jsonContract)
        {
            return typeof(IActionResult).IsAssignableFrom(jsonContract.UnderlyingType);
        }
    }
}