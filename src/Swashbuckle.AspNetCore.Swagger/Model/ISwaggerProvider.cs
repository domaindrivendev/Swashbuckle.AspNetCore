using System;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.AspNetCore.Swagger
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwagger(
            string documentName,
            string host = null,
            string basePath = null,
            string[] schemes = null);
    }

    public class UnknownSwaggerDocument : Exception
    {
        public UnknownSwaggerDocument(string documentName, IEnumerable<string> knownDocuments)
            : base(string.Format("Unknown Swagger document - \"{0}\". Known Swagger documents: {1}{2}",
                documentName,
                Environment.NewLine,
                string.Join(Environment.NewLine, knownDocuments?.Select(x => $"\"{x}\""))))
        {}
    }
}