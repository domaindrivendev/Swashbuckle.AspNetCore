using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.ApiDescription
{
    /// <summary>
    /// This service will be looked up by name from the service collection when using
    /// the Microsoft.Extensions.ApiDescription tool. Public only for testing.
    /// </summary>
    public interface IDocumentProvider
    {
        Task GenerateAsync(string documentName, TextWriter writer);
    }
}
