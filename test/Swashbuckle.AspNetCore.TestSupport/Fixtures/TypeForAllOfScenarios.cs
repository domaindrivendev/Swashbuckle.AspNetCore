using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeForAllOfScenarios
    {
        public TypeForAllOfScenarios SelfReferencingScenario { get; set; }

        public Dictionary<string, TypeForAllOfScenarios> DictionaryScenario { get; set; }

        public List<TypeForAllOfScenarios> ListScenario { get; set; }
    }
}
