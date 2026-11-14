using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

// A paged result: it streams its items, and also has a property of its own
public class AsyncEnumerableOfComplexType : IAsyncEnumerable<ComplexType>
{
    public int Total { get; set; }

    public IAsyncEnumerator<ComplexType> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
