namespace Microsoft.Extensions.ApiDescriptions;

/// <summary>
/// This service will be looked up by name from the service collection when using
/// the <c>dotnet-getdocument</c> tool from the Microsoft.Extensions.ApiDescription.Server package.
/// </summary>
internal interface IDocumentProvider
{
    IEnumerable<string> GetDocumentNames();

    Task GenerateAsync(string documentName, TextWriter writer);
}
