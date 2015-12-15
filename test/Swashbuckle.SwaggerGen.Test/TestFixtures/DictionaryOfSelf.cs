using System.Collections.Generic;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class DictionaryOfSelf : Dictionary<string, DictionaryOfSelf>
    {
    }
}