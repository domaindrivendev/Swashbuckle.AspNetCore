using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.SwaggerGen.Generator
{
    internal static class JsonContractExtensions
    {
        private static IEnumerable<string> AmbiguousTypeNames = new[]
        {
                "System.Object"
        };

        internal static bool IsSelfReferencing(this JsonDictionaryContract dictionaryContract)
        {
            return dictionaryContract.UnderlyingType == dictionaryContract.DictionaryValueType;
        }

        internal static bool IsSelfReferencing(this JsonArrayContract arrayContract)
        {
            return arrayContract.UnderlyingType == arrayContract.CollectionItemType;
        }

        internal static bool IsAmbiguous(this JsonObjectContract objectContract)
        {
            return AmbiguousTypeNames.Contains(objectContract.UnderlyingType.FullName);
        }
    }
}