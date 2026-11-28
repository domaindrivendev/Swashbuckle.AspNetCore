using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class TypeWithAsyncEnumerableProperty
{
    public int Total { get; set; }

    public IAsyncEnumerable<ComplexType> Results { get; set; }
}
