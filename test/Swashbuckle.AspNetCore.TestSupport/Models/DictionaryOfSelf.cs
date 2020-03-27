using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class DictionaryOfSelf : Dictionary<string, DictionaryOfSelf>
    {
    }
}