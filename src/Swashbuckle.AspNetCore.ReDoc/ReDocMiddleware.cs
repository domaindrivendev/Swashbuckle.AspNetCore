using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#if NETCOREAPP3_0
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
#endif

namespace Swashbuckle.AspNetCore.ReDoc
{
    public class ReDocMiddleware
    {
        private const string EmbeddedFileNamespace = "Swashbuckle.AspNetCore.ReDoc.node_modules.redoc.bundles";

        private readonly ReDocOptions _options;
        private readonly JsonSerializer _jsonSerializer;
        private readonly StaticFileMiddleware _staticFileMiddleware;

        public ReDocMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            IOptions<ReDocOptions> optionsAccessor)
            : this(next, hostingEnv, loggerFactory, optionsAccessor.Value)
        { }

        public ReDocMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            ReDocOptions options)
        {
            _options = options ?? new ReDocOptions();
            _jsonSerializer = CreateJsonSerializer();
            _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory, options);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;

            // If the RoutePrefix is requested (with or without trailing slash), redirect to index URL
            if (httpMethod == "GET" && Regex.IsMatch(path, $"^/{_options.RoutePrefix}/?$"))
            {
                // Use relative redirect to support proxy environments
                var relativeRedirectPath = path.EndsWith("/")
                    ? "index.html"
                    : $"{path.Split('/').Last()}/index.html";

                RespondWithRedirect(httpContext.Response, relativeRedirectPath);
                return;
            }

            if (httpMethod == "GET" && Regex.IsMatch(path, $"/{_options.RoutePrefix}/?index.html"))
            {
                await RespondWithIndexHtml(httpContext.Response);
                return;
            }

            await _staticFileMiddleware.Invoke(httpContext);
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

        private StaticFileMiddleware CreateStaticFileMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            ReDocOptions options)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(ReDocMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }

        private void RespondWithRedirect(HttpResponse response, string location)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = location;
        }

        private async Task RespondWithIndexHtml(HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";

            using (var stream = _options.IndexStream())
            {
                // Inject arguments before writing to response
                var htmlBuilder = new StringBuilder(new StreamReader(stream).ReadToEnd());
                foreach (var entry in GetIndexArguments())
                {
                    htmlBuilder.Replace(entry.Key, entry.Value);
                }

                await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
            }
        }

        private IDictionary<string, string> GetIndexArguments()
        {
            return new Dictionary<string, string>()
            {
                { "%(DocumentTitle)", _options.DocumentTitle },
                { "%(HeadContent)", _options.HeadContent },
                { "%(SpecUrl)", _options.SpecUrl },
                { "%(ConfigObject)", SerializeToJson(_options.ConfigObject) }
            };
        }

        private string SerializeToJson(object obj)
        {
            var writer = new StringWriter();
            _jsonSerializer.Serialize(writer, obj);
            return writer.ToString();
        }
    }
}