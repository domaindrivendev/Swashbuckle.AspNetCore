namespace Swashbuckle.AspNetCore.Swagger;

public class UnknownSwaggerDocument(string documentName, IEnumerable<string> knownDocuments)
    : InvalidOperationException(
        string.Format(
            "Unknown Swagger document - \"{0}\". Known Swagger documents: {1}",
            documentName,
            string.Join(",", knownDocuments?.Select(x => $"\"{x}\""))));
