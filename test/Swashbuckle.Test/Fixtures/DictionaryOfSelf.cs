using System.Collections.Generic;

namespace Swashbuckle.Test.Fixtures
{
    public class DictionaryOfSelf : Dictionary<string, DictionaryOfSelf>
    {
    }
}