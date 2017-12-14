using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerUI3
{
    public class SwaggerUIFileProvider : IFileProvider
    {
        private const string StaticFilesNamespace =
            "Swashbuckle.AspNetCore.SwaggerUI3.bower_components.swagger_ui.dist";
        private const string IndexResourceName =
            "Swashbuckle.AspNetCore.SwaggerUI3.Template.index.html";

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