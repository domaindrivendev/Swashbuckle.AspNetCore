using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class JsonContractExtensions
    {
        internal static bool IsSelfReferencingArrayOrDictionary(this JsonContract jsonContract)
        {
            var arrayContract = jsonContract as JsonArrayContract;
            if (arrayContract != null)
                return arrayContract.UnderlyingType == arrayContract.CollectionItemType;

            var dictionaryContract = jsonContract as JsonDictionaryContract;
            if (dictionaryContract != null)
                return dictionaryContract.UnderlyingType == dictionaryContract.DictionaryValueType;

            return false;
        }
    }
}