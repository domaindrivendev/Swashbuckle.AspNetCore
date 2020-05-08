using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Extensions;
#if NETCOREAPP3_0
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
#endif

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIMiddleware
    {
        private const string EmbeddedFileNamespace = "Swashbuckle.AspNetCore.SwaggerUI.node_modules.swagger_ui_dist";

        private readonly SwaggerUIOptions _options;
        private readonly StaticFileMiddleware _staticFileMiddleware;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SwaggerUIMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            SwaggerUIOptions options)
        {
            _options = options ?? new SwaggerUIOptions();

            _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory, options);

            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            _jsonSerializerOptions.IgnoreNullValues = true;
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;

            // If the RoutePrefix is requested (with or without trailing slash), redirect to index URL
            if (httpMethod == "GET" && Regex.IsMatch(path, $"^/?{Regex.Escape(_options.RoutePrefix)}/?$",  RegexOptions.IgnoreCase))
            {
                var indexUrl = httpContext.Request.GetEncodedUrl().TrimEnd('/') + "/index.html";

                RespondWithRedirect(httpContext.Response, indexUrl);
                return;
            }

            if (httpMethod == "GET" && Regex.IsMatch(path, $"^/{Regex.Escape(_options.RoutePrefix)}/?index.html$",  RegexOptions.IgnoreCase))
            {
                await RespondWithIndexHtml(httpContext);
                return;
            }

            await _staticFileMiddleware.Invoke(httpContext);
        }

        private static StaticFileMiddleware CreateStaticFileMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            SwaggerUIOptions options)
        {
            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUIMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }

        private static void RespondWithRedirect(HttpResponse response, string location)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = location;
        }

        protected virtual async Task RespondWithIndexHtml(HttpContext httpContext)
        {
            var response = httpContext.Response;
            response.StatusCode = 200;
            response.ContentType = "text/html;charset=utf-8";
            var htmlBuilder = new StringBuilder();
            using (var stream = _options.IndexStream())
            using (var reader = new StreamReader(stream))
            {
                // Inject arguments before writing to response
                htmlBuilder.Append(await reader.ReadToEndAsync());

                foreach (var entry in GetIndexArguments(httpContext))
                {
                    htmlBuilder.Replace(entry.Key, entry.Value);
                }
            }

            if (_options.CspConfigObject.GenerateHeader)
            {
                var scriptNonce = GetNonce(httpContext, _options.CspConfigObject.ScriptNonceKey);
                var styleNonce = GetNonce(httpContext, _options.CspConfigObject.StyleNonceKey);
                var cspHeaderName = CspHeaderName(_options.CspConfigObject.ReportOnly);
                response.Headers[cspHeaderName] = _options.CspConfigObject.HeaderGenerator(scriptNonce, styleNonce);
            }

            await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
        }

        protected virtual IDictionary<string, string> GetIndexArguments(HttpContext httpContext)
        {
            var ret = new Dictionary<string, string>
            {
                { "%(DocumentTitle)", _options.DocumentTitle },
                { "%(ConfigObject)", JsonSerializer.Serialize(_options.ConfigObject, _jsonSerializerOptions) },
                { "%(OAuthConfigObject)", JsonSerializer.Serialize(_options.OAuthConfigObject, _jsonSerializerOptions) }
            };

            var headerContent = new StringBuilder(_options.HeadContent);
            
            var scriptNonce = GetNonce(httpContext, _options.CspConfigObject.ScriptNonceKey);
            headerContent.Replace("%(ScriptNonce)", scriptNonce);
            ret.Add("%(ScriptNonce)", scriptNonce);

            var styleNonce = GetNonce(httpContext, _options.CspConfigObject.StyleNonceKey);
            headerContent.Replace("%(StyleNonce)", styleNonce);
            ret.Add("%(StyleNonce)", styleNonce);

            ret.Add("%(HeadContent)", headerContent.ToString());
            return ret;
        }

        private static string GetNonce(HttpContext httpContext, string key)
        {
            if (!(httpContext.Items[key] is string nonce))
            {
                nonce =  GenerateNonce();
                httpContext.Items[key] = nonce;
            }

            return nonce;
        }

        private static string GenerateNonce() =>
            Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", string.Empty);

        private static string CspHeaderName(bool reportOnly) =>
            reportOnly ? "Content-Security-Policy-Report-Only" : "Content-Security-Policy";
    }
}
