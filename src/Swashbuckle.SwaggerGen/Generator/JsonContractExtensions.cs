using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.SwaggerGen.Generator
{
    public static class JsonContractExtensions
    {
        private static IEnumerable<string> AmbiguousTypeNames = new[]
        {
                "System.Object"
        };

        public static bool IsSelfReferencing(this JsonDictionaryContract dictionaryContract)
        {
            return dictionaryContract.UnderlyingType == dictionaryContract.DictionaryValueType;
        }

        public static bool IsSelfReferencing(this JsonArrayContract arrayContract)
        {
            return arrayContract.UnderlyingType == arrayContract.CollectionItemType;
        }

        public static bool IsAmbiguous(this JsonObjectContract objectContract)
        {
            return AmbiguousTypeNames.Contains(objectContract.UnderlyingType.FullName);
        }
    }
}