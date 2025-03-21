namespace Swashbuckle.AspNetCore.Swagger;

public class UnknownSwaggerDocument : InvalidOperationException
{
    public UnknownSwaggerDocument(string documentName, IEnumerable<string> knownDocuments)
        : base(string.Format("Unknown Swagger document - \"{0}\". Known Swagger documents: {1}",
            documentName,
            string.Join(",", knownDocuments?.Select(x => $"\"{x}\""))))
    {
    }
}
