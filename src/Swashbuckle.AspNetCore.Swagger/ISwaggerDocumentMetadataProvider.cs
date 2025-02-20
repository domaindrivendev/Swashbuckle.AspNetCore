using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.Swagger
{
    public interface ISwaggerDocumentMetadataProvider
    {
        IList<string> GetDocumentNames();
    }
}
