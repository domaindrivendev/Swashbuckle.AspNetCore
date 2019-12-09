using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class DictionaryOfSelf : Dictionary<string, DictionaryOfSelf>
    {
    }
}