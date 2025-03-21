using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

namespace Swashbuckle.AspNetCore.ReDoc;

internal sealed class ReDocMiddleware
{
    private const string EmbeddedFileNamespace = "Swashbuckle.AspNetCore.ReDoc.node_modules.redoc.bundles";

    private readonly ReDocOptions _options;
    private readonly StaticFileMiddleware _staticFileMiddleware;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ReDocMiddleware(
        RequestDelegate next,
        IWebHostEnvironment hostingEnv,
        ILoggerFactory loggerFactory,
        ReDocOptions options)
    {
        _options = options ?? new ReDocOptions();

        _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory, options);

        if (options.JsonSerializerOptions != null)
        {
            _jsonSerializerOptions = options.JsonSerializerOptions;
        }
#if !NET6_0_OR_GREATER
        else
        {
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
#if NET5_0_OR_GREATER
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
                IgnoreNullValues = true,
#endif
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) }
            };
        }
#endif
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

    private static StaticFileMiddleware CreateStaticFileMiddleware(
        RequestDelegate next,
        IWebHostEnvironment hostingEnv,
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

    private static void RespondWithRedirect(HttpResponse response, string location)
    {
        response.StatusCode = StatusCodes.Status301MovedPermanently;
#if NET6_0_OR_GREATER
        response.Headers.Location = location;
#else
        response.Headers["Location"] = location;
#endif
    }

    private async Task RespondWithFile(HttpResponse response, string fileName)
    {
        response.StatusCode = 200;

        Stream stream;

        switch (fileName)
        {
            case "index.css":
                response.ContentType = "text/css";
                stream = ResourceHelper.GetEmbeddedResource(fileName);
                break;
            case "index.js":
                response.ContentType = "application/javascript;charset=utf-8";
                stream = ResourceHelper.GetEmbeddedResource(fileName);
                break;
            default:
                response.ContentType = "text/html;charset=utf-8";
                stream = _options.IndexStream();
                break;
        }

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

#if NET5_0_OR_GREATER
    [UnconditionalSuppressMessage(
        "AOT",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Method is only called if the user provides their own custom JsonSerializerOptions.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Method is only called if the user provides their own custom JsonSerializerOptions.")]
#endif
    private Dictionary<string, string> GetIndexArguments()
    {
        string configObject = null;

#if NET6_0_OR_GREATER
        if (_jsonSerializerOptions is null)
        {
            configObject = JsonSerializer.Serialize(_options.ConfigObject, ReDocOptionsJsonContext.Default.ConfigObject);
        }
#endif

        configObject ??= JsonSerializer.Serialize(_options.ConfigObject, _jsonSerializerOptions);

        return new Dictionary<string, string>()
        {
            { "%(DocumentTitle)", _options.DocumentTitle },
            { "%(HeadContent)", _options.HeadContent },
            { "%(SpecUrl)", _options.SpecUrl },
            { "%(ConfigObject)", configObject },
        };
    }
}
