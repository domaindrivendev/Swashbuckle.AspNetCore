using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIFileProvider : IFileProvider
    {
        private const string StaticFilesNamespace =
            "Swashbuckle.AspNetCore.SwaggerUi.bower_components.swagger_ui.dist";
        private const string IndexResourceName =
            "Swashbuckle.AspNetCore.SwaggerUi.Template.index.html";

        private readonly Assembly _thisAssembly;
        private readonly EmbeddedFileProvider _staticFileProvider;
        private readonly IDictionary<string, string> _indexParameters;

        public SwaggerUIFileProvider(IDictionary<string, string> indexParameters)
        {
            _thisAssembly = GetType().GetTypeInfo().Assembly;
            _staticFileProvider = new EmbeddedFileProvider(_thisAssembly, StaticFilesNamespace);
            _indexParameters = indexParameters;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _staticFileProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == "/index.html")
                return new SwaggerUIIndexFileInfo(_thisAssembly, IndexResourceName, _indexParameters);

            return _staticFileProvider.GetFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _staticFileProvider.Watch(filter);
        }
    }
}