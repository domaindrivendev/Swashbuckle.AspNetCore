using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIIndexHtmlBuilder
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly SwaggerUIOptions _options;

        public SwaggerUIIndexHtmlBuilder(SwaggerUIOptions options)
        {
            _options = options ?? new SwaggerUIOptions();
            _jsonSerializer = CreateJsonSerializer();
        }

        /// <summary>
        /// Gets the HTML-content of the index-page, as specified in the provided
        /// <see cref="SwaggerUIOptions.IndexStream"/> in the <see cref="SwaggerUIOptions"/>.
        /// </summary>
        /// <returns></returns>
        public string Build()
        {
            StringBuilder htmlBuilder;
            var indexContent = string.Empty;

            using (var stream = _options.IndexStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    indexContent = reader.ReadToEnd();
                }
            }

            // Inject arguments before writing to response
            htmlBuilder = new StringBuilder(indexContent);
            foreach (var entry in GetIndexArguments())
            {
                htmlBuilder.Replace(entry.Key, entry.Value);
            }

            return htmlBuilder.ToString();
        }

        private IDictionary<string, string> GetIndexArguments()
        {
            var trimmedBasePath = $"{_options.BasePath.TrimEnd('/')}/";

            return new Dictionary<string, string>()
            {
                { "%(DocumentTitle)", _options.DocumentTitle },
                { "%(HeadContent)", _options.HeadContent },
                { "%(BasePath)", trimmedBasePath },
                { "%(ConfigObject)", SerializeToJson(_options.ConfigObject) },
                { "%(OAuthConfigObject)", SerializeToJson(_options.OAuthConfigObject) },
            };
        }

        private string SerializeToJson(object obj)
        {
            var writer = new StringWriter();
            _jsonSerializer.Serialize(writer, obj);
            return writer.ToString();
        }

        private JsonSerializer CreateJsonSerializer()
        {
            return JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter(true) },
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
        }
    }
}
