#nullable enable

using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.Net.Http.Headers;

#if NET
using System.Collections.Frozen;
#endif

namespace Swashbuckle.AspNetCore;

internal class CompressedEmbeddedFileResponser
{
    private readonly Assembly _assembly;
    private readonly string _cacheControlHeaderValue;
    private readonly string _pathPrefix;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

#if NET

    private readonly FrozenDictionary<string, ResourceIndexCache> _resourceIndexes;
#else
    private readonly Dictionary<string, ResourceIndexCache> _resourceIndexes;
#endif

    public CompressedEmbeddedFileResponser(Assembly assembly, string resourceNamePrefix, string pathPrefix, TimeSpan? cacheLifetime)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _pathPrefix = string.IsNullOrWhiteSpace(pathPrefix)
                      ? string.Empty
                      : pathPrefix == "/"
                        ? string.Empty
                        : pathPrefix.TrimEnd('/');
        _cacheControlHeaderValue = GetCacheControlHeaderValue(cacheLifetime);

        var resourceIndexes = assembly.GetManifestResourceNames()
                                      .Where(name => name.StartsWith(resourceNamePrefix, StringComparison.Ordinal))
                                      .ToDictionary(name => name.Substring(resourceNamePrefix.Length), name => new ResourceIndexCache(name), StringComparer.Ordinal);

#if NET
        _resourceIndexes = resourceIndexes.ToFrozenDictionary();
#else
        _resourceIndexes = resourceIndexes;
#endif
    }

    public async Task<bool> TryRespondWithFileAsync(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value?.ToString() ?? string.Empty;
        if (!path.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        path = path.Substring(_pathPrefix.Length).Replace('/', '.');

        if (_resourceIndexes.TryGetValue(path, out var resourceIndexCache))
        {
            var contentType = GetContentType(resourceIndexCache);
            var (etag, Length) = GetDecompressContentETag(resourceIndexCache);

            var responseHeaders = httpContext.Response.Headers;
#if NET
            var ifNoneMatch = httpContext.Request.Headers.IfNoneMatch.ToString();
#else
            var ifNoneMatch = httpContext.Request.Headers["If-None-Match"].ToString();
#endif
            if (ifNoneMatch == etag)
            {
                httpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                return true;
            }

            var responseWithGZip = httpContext.IsGZipAccepted();
            if (responseWithGZip)
            {
#if NET
                responseHeaders.ContentEncoding = "gzip";
#else
                responseHeaders["Content-Encoding"] = "gzip";
#endif
            }

#if NET
            responseHeaders.ContentType = contentType;
            responseHeaders.ETag = etag;
            responseHeaders.CacheControl = _cacheControlHeaderValue;
#else
            responseHeaders["Content-Type"] = contentType;
            responseHeaders["ETag"] = etag;
            responseHeaders["Cache-Control"] = _cacheControlHeaderValue;
#endif
            using var stream = OpenResourceStream(resourceIndexCache);
            if (responseWithGZip)
            {
                responseHeaders.ContentLength = stream.Length;
                await stream.CopyToAsync(httpContext.Response.Body, 81320, httpContext.RequestAborted);
            }
            else
            {
                responseHeaders.ContentLength = Length;
                using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
                await gzipStream.CopyToAsync(httpContext.Response.Body, 81320, httpContext.RequestAborted);
            }

            return true;
        }
        return false;
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
        //Get decompress content and hash
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

        using var md5 = MD5.Create();
        var hashData = md5.ComputeHash(memoryStream);

        resourceIndexCache.ETag = $"\"{Convert.ToBase64String(hashData)}\"";

        return (resourceIndexCache.ETag, resourceIndexCache.DecompressContentLength.Value);
    }

    private Stream OpenResourceStream(ResourceIndexCache resourceIndexCache)
    {
        // Actually, since the name comes from GetManifestResourceNames(), the content can definitely be obtained
        return _assembly.GetManifestResourceStream(resourceIndexCache.ResourceName)
               ?? throw new InvalidOperationException($"Can not read resource \"{resourceIndexCache.ResourceName}\" in {_assembly}");
    }

    private class ResourceIndexCache(string resourceName)
    {
        public string? ETag { get; set; }

        public string? ContentType { get; set; }

        public long? DecompressContentLength { get; set; }

        public string ResourceName { get; } = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
    }
}
