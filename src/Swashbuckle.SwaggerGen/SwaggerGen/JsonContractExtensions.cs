using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.SwaggerGen
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
    }
}