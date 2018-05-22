using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class JsonContractExtensions
    {
        internal static bool IsSelfReferencingArrayOrDictionary(this JsonContract jsonContract)
        {
            if (jsonContract is JsonArrayContract arrayContract)
                return arrayContract.UnderlyingType == arrayContract.CollectionItemType;

            if (jsonContract is JsonDictionaryContract dictionaryContract)
                return dictionaryContract.UnderlyingType == dictionaryContract.DictionaryValueType;

            return false;
        }
    }
}