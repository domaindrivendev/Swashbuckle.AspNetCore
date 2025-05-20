using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

#if NET
using System.Diagnostics.CodeAnalysis;
#else
using System.Text.Json.Serialization;
#endif

namespace Swashbuckle.AspNetCore.ReDoc;

internal sealed class ReDocMiddleware
{
    private const string EmbeddedFileNamespace = "Swashbuckle.AspNetCore.ReDoc.node_modules.redoc.bundles";

    private static readonly string ReDocVersion = GetReDocVersion();

    private readonly RequestDelegate _next;
    private readonly ReDocOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly CompressedEmbeddedFileResponder _compressedEmbeddedFileResponder;

    public ReDocMiddleware(
        RequestDelegate next,
        ReDocOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? new ReDocOptions();

        if (options.JsonSerializerOptions != null)
        {
            _jsonSerializerOptions = options.JsonSerializerOptions;
        }
#if !NET
        else
        {
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) }
            };
        }
#endif

        var pathPrefix = options.RoutePrefix.StartsWith("/") ? options.RoutePrefix : $"/{options.RoutePrefix}";
        _compressedEmbeddedFileResponder = new(typeof(ReDocMiddleware).Assembly, EmbeddedFileNamespace, pathPrefix, _options.CacheLifetime);
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
                var relativeIndexUrl =
#if NET
                    string.IsNullOrEmpty(path) || path.EndsWith('/')
#else
                    string.IsNullOrEmpty(path) || path.EndsWith("/")
#endif
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

        if (!await _compressedEmbeddedFileResponder.TryRespondWithFileAsync(httpContext))
        {
            await _next(httpContext);
        }
    }

    private static string GetReDocVersion()
    {
        return typeof(ReDocMiddleware).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where((p) => p.Key is "ReDocVersion")
            .Select((p) => p.Value)
            .DefaultIfEmpty(string.Empty)
            .FirstOrDefault();
    }

    private static void SetCacheHeaders(HttpResponse response, ReDocOptions options, string etag = null)
    {
        var headers = response.GetTypedHeaders();

        if (options.CacheLifetime is { } maxAge)
        {
            headers.CacheControl = new()
            {
                MaxAge = maxAge,
                Private = true,
            };
        }
        else
        {
            headers.CacheControl = new()
            {
                NoCache = true,
                NoStore = true,
            };
        }

        headers.ETag = new($"\"{etag ?? ReDocVersion}\"", isWeak: true);
    }

    private static void RespondWithRedirect(HttpResponse response, string location)
    {
        response.StatusCode = StatusCodes.Status301MovedPermanently;
#if NET
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

            var text = content.ToString();
            var etag = HashText(text);

            SetCacheHeaders(response, _options, etag);

            await response.WriteAsync(text, Encoding.UTF8);
        }
    }

    private static string HashText(string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);

#if NET
        var hash = SHA1.HashData(buffer);
#else
        using var sha = SHA1.Create();
        var hash = sha.ComputeHash(buffer);
#endif

        return Convert.ToBase64String(hash);
    }

#if NET
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

#if NET
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
