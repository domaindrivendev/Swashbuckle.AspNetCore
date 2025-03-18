namespace Swashbuckle.AspNetCore.Swagger
{
    public interface ISwaggerDocumentMetadataProvider
    {
        IList<string> GetDocumentNames();
    }
}
