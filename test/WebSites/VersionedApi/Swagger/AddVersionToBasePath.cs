using System.Linq;
using Swashbuckle.Swagger;

namespace VersionedApi.Swagger
{
    public class AddVersionToBasePath : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.basePath = "/" + swaggerDoc.info.version;

            swaggerDoc.paths = swaggerDoc.paths.ToDictionary(
                entry => entry.Key.Replace("/{version}", ""),
                entry =>
                {
                    var pathItem = entry.Value;
                    RemoveVersionParamFrom(pathItem.get);
                    RemoveVersionParamFrom(pathItem.put);
                    RemoveVersionParamFrom(pathItem.post);
                    RemoveVersionParamFrom(pathItem.delete);
                    RemoveVersionParamFrom(pathItem.options);
                    RemoveVersionParamFrom(pathItem.head);
                    RemoveVersionParamFrom(pathItem.patch);
                    return pathItem;
                });
        }

        private void RemoveVersionParamFrom(Operation operation)
        {
            if (operation == null || operation.parameters == null) return;

            var versionParam = operation.parameters.FirstOrDefault(param => param.name == "version");
            if (versionParam == null) return;

            operation.parameters.Remove(versionParam) ;
        }
    }
}