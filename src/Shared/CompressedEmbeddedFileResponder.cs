#nullable enable

using System.Collections.Frozen;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Swashbuckle.AspNetCore;

internal class CompressedEmbeddedFileResponder
{
    private readonly Assembly _assembly;

    private readonly StringValues _cacheControlHeaderValue;

    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    private readonly string _pathPrefix;

    private readonly FrozenDictionary<string, ResourceIndexCache> _resourceMap;

    public CompressedEmbeddedFileResponder(Assembly assembly, string resourceNamePrefix, string pathPrefix, TimeSpan? cacheLifetime)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _pathPrefix = pathPrefix.TrimEnd('/');
        _cacheControlHeaderValue = GetCacheControlHeaderValue(cacheLifetime);

        var resourceMap = assembly.GetManifestResourceNames()
                                  .Where(name => name.StartsWith(resourceNamePrefix, StringComparison.Ordinal))
                                  .ToDictionary(name => name.Substring(resourceNamePrefix.Length), name => new ResourceIndexCache(name), StringComparer.Ordinal);

        _resourceMap = resourceMap.ToFrozenDictionary();
    }

    public async Task<bool> TryRespondWithFileAsync(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value?.ToString() ?? string.Empty;
        if (!path.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        path = path.Substring(_pathPrefix.Length).Replace('/', '.');

        if (!_resourceMap.TryGetValue(path, out var resourceIndexCache))
        {
            return false;
        }

        var contentType = GetContentType(resourceIndexCache);
        var (etag, Length) = GetDecompressContentETag(resourceIndexCache);

        var responseHeaders = httpContext.Response.Headers;
        var ifNoneMatch = httpContext.Request.Headers.IfNoneMatch.ToString();
        if (ifNoneMatch == etag)
        {
            httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
            return true;
        }

        var responseWithGZip = httpContext.IsGZipAccepted();
        if (responseWithGZip)
        {
            responseHeaders.ContentEncoding = "gzip";
        }

        responseHeaders.ContentType = contentType;
        responseHeaders.ETag = etag;
        responseHeaders.CacheControl = _cacheControlHeaderValue;

        using var stream = OpenResourceStream(resourceIndexCache);
        if (responseWithGZip)
        {
            responseHeaders.ContentLength = stream.Length;
            await stream.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
        }
        else
        {
            responseHeaders.ContentLength = Length;
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            await gzipStream.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
        }

        return true;
    }

    private static string GetCacheControlHeaderValue(TimeSpan? cacheLifetime)
    {
        if (cacheLifetime is { } maxAge)
        {
            return new CacheControlHeaderValue()
            {
                MaxAge = maxAge,
                Private = true,
            }.ToString();
        }
        else
        {
            return new CacheControlHeaderValue()
            {
                NoCache = true,
                NoStore = true,
            }.ToString();
        }
    }

    private string GetContentType(ResourceIndexCache resourceIndexCache)
    {
        return resourceIndexCache.ContentType
               ?? (_contentTypeProvider.TryGetContentType(resourceIndexCache.ResourceName, out var contentTypeValue)
                   ? contentTypeValue
                   : "application/octet-stream");
    }

    private (string ETag, long DecompressContentLength) GetDecompressContentETag(ResourceIndexCache resourceIndexCache)
    {
        if (resourceIndexCache.ETag != null
            && resourceIndexCache.DecompressContentLength != null)
        {
            return (resourceIndexCache.ETag, resourceIndexCache.DecompressContentLength.Value);
        }

        using var stream = OpenResourceStream(resourceIndexCache);

        using var memoryStream = new MemoryStream((int)stream.Length * 2);
        using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
        gzipStream.CopyTo(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        resourceIndexCache.DecompressContentLength = memoryStream.Length;

        var hashData = SHA1.HashData(memoryStream);

        resourceIndexCache.ETag = $"\"{Convert.ToBase64String(hashData)}\"";

        return (resourceIndexCache.ETag, resourceIndexCache.DecompressContentLength.Value);
    }

    private Stream OpenResourceStream(ResourceIndexCache resourceIndexCache)
    {
        // Actually, since the name comes from GetManifestResourceNames(), the content can definitely be obtained
        return _assembly.GetManifestResourceStream(resourceIndexCache.ResourceName)!;
    }

    private sealed class ResourceIndexCache(string resourceName)
    {
        public string? ContentType { get; set; }

        public long? DecompressContentLength { get; set; }

        public string? ETag { get; set; }

        public string ResourceName { get; } = resourceName;
    }
}
