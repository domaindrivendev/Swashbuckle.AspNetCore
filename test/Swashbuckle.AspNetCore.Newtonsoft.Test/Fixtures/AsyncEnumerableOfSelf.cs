namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

// A node that streams nodes of its own type, and also has a property of its own
public class AsyncEnumerableOfSelf : IAsyncEnumerable<AsyncEnumerableOfSelf>
{
    public int Id { get; set; }

    public IAsyncEnumerator<AsyncEnumerableOfSelf> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
