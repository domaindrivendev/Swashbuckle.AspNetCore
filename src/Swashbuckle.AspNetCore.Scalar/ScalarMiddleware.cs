using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#if (NETSTANDARD2_0)
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#endif

namespace Swashbuckle.AspNetCore.Scalar
{
    public class ScalarMiddleware
    {
        private const string EmbeddedFileNamespace = "Swashbuckle.AspNetCore.Scalar.node_modules._scalar.api_reference.dist.browser";

        private readonly ScalarOptions _options;
        private readonly StaticFileMiddleware _staticFileMiddleware;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ScalarMiddleware(
            RequestDelegate next,
            IWebHostEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            ScalarOptions options)
        {
            _options = options ?? new ScalarOptions();

            _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory, options);

            _jsonSerializerOptions = new JsonSerializerOptions();

#if NET6_0_OR_GREATER
            _jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
#else
            _jsonSerializerOptions.IgnoreNullValues = true;
#endif
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;

            if (HttpMethods.IsGet(httpMethod))
            {
                var path = httpContext.Request.Path.Value;

                // If the RoutePrefix is requested (with or without trailing slash), redirect to index URL
                if (Regex.IsMatch(path, $"^/?{Regex.Escape(_options.RoutePrefix)}/?$", RegexOptions.IgnoreCase))
                {
                    // Use relative redirect to support proxy environments
                    var relativeIndexUrl = string.IsNullOrEmpty(path) || path.EndsWith("/")
                        ? "index.html"
                        : $"{path.Split('/').Last()}/index.html";

                    RespondWithRedirect(httpContext.Response, relativeIndexUrl);
                    return;
                }

                var match = Regex.Match(path, $"^/{Regex.Escape(_options.RoutePrefix)}/?(index.(html|css|js))$", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    await RespondWithFile(httpContext.Response, match.Groups[1].Value);
                    return;
                }
            }

            await _staticFileMiddleware.Invoke(httpContext);
        }

        private StaticFileMiddleware CreateStaticFileMiddleware(
            RequestDelegate next,
            IWebHostEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            ScalarOptions options)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(ScalarMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }

        private void RespondWithRedirect(HttpResponse response, string location)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = location;
        }

        private async Task RespondWithFile(HttpResponse response, string fileName)
        {
            response.StatusCode = 200;

            Stream stream;

            response.ContentType = "text/html;charset=utf-8";
            stream = _options.IndexStream();

            using (stream)
            {
                // Inject arguments before writing to response
                var content = new StringBuilder(new StreamReader(stream).ReadToEnd());
                foreach (var entry in GetIndexArguments())
                {
                    content.Replace(entry.Key, entry.Value);
                }

                await response.WriteAsync(content.ToString(), Encoding.UTF8);
            }
        }

        private IDictionary<string, string> GetIndexArguments()
        {
            return new Dictionary<string, string>()
            {
                { "%(DocumentTitle)", _options.DocumentTitle },
                { "%(HeadContent)", _options.HeadContent },
                { "%(SpecUrl)", _options.SpecUrl },
                { "%(ConfigObject)",
                    // Serialize twice to obtain an escaped json string.
                    JsonSerializer.Serialize(JsonSerializer.Serialize(_options.Config, _jsonSerializerOptions)) }
            };
        }
    }
}
