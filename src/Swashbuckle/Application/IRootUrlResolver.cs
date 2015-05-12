using Microsoft.AspNet.Http;

namespace Swashbuckle.Application
{
    public interface IRootUrlResolver
    {
        string ResolveFrom(HttpRequest request);
    }
}
