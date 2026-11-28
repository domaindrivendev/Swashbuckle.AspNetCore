using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class FakeControllerWithAsyncEnumerable
{
    public IAsyncEnumerable<ComplexType> ReturnsAsyncEnumerable()
        => throw new NotImplementedException();

    public AsyncEnumerableOfComplexType ReturnsTypeImplementingAsyncEnumerable()
        => throw new NotImplementedException();

    public TypeWithAsyncEnumerableProperty ReturnsTypeWithAsyncEnumerableProperty()
        => throw new NotImplementedException();

    public AsyncEnumerableOfSelf ReturnsAsyncEnumerableOfSelf(AsyncEnumerableOfSelf body)
        => throw new NotImplementedException();
}
