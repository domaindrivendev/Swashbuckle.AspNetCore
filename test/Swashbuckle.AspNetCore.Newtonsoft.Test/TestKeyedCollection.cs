using System.Collections.ObjectModel;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class TestKeyedCollection : KeyedCollection<string, TestDto>
{
    protected override string GetKeyForItem(TestDto item) => item.Prop1;
}
