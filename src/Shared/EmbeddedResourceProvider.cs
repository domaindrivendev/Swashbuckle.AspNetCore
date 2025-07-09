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

internal sealed class EmbeddedResourceProvider(
    Assembly assembly,
    string resourceNamePrefix,
    string pathPrefix,
    TimeSpan? cacheLifetime)
{
    private const string GZipEncodingValue = "gzip";
    private static readonly StringValues GZipEncodingHeader = new(GZipEncodingValue);

    private readonly Assembly _assembly = assembly;
    private readonly StringValues _cacheControl = GetCacheControlHeader(cacheLifetime);
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();
    private readonly string _pathPrefix = pathPrefix.TrimEnd('/');

    private readonly FrozenDictionary<string, ResourceEntry> _resourceCache = assembly
        .GetManifestResourceNames()
        .Where((p) => p.StartsWith(resourceNamePrefix, StringComparison.Ordinal))
        .ToFrozenDictionary((p) => p[resourceNamePrefix.Length..], name => new ResourceEntry(name), StringComparer.Ordinal);

    public async Task<bool> TryRespondWithFileAsync(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        path = path[_pathPrefix.Length..].Replace('/', '.');

        if (!_resourceCache.TryGetValue(path, out var cacheEntry))
        {
            return false;
        }

        var contentType = GetContentType(cacheEntry);
        var (etag, supportsCompression) = GetContentProperties(cacheEntry);

        var response = httpContext.Response;
        var ifNoneMatch = httpContext.Request.Headers.IfNoneMatch;

        if (ifNoneMatch == etag)
        {
            response.StatusCode = StatusCodes.Status304NotModified;
            return true;
        }

        var serveCompressed = supportsCompression && IsGZipAccepted(httpContext.Request);
        var responseHeaders = response.Headers;

        if (serveCompressed)
        {
            responseHeaders.ContentEncoding = GZipEncodingHeader;
        }

        responseHeaders.CacheControl = _cacheControl;
        responseHeaders.ContentLength = serveCompressed ? cacheEntry.CompressedLength : cacheEntry.DecompressedLength;
        responseHeaders.ContentType = contentType;
        responseHeaders.ETag = etag;

        using var compressed = GetResource(cacheEntry);

        if (serveCompressed)
        {
            await compressed.CopyToAsync(response.Body, httpContext.RequestAborted);
        }
        else
        {
            using var decompressed = new GZipStream(compressed, CompressionMode.Decompress);
            await decompressed.CopyToAsync(response.Body, httpContext.RequestAborted);
        }

        return true;
    }

    private static bool IsGZipAccepted(HttpRequest httpRequest)
    {
        if (httpRequest.GetTypedHeaders().AcceptEncoding is not { Count: > 0 } acceptEncoding)
        {
            return false;
        }

        for (int i = 0; i < acceptEncoding.Count; i++)
        {
            var encoding = acceptEncoding[i];

            if (encoding.Quality is not 0 &&
                string.Equals(encoding.Value.Value, GZipEncodingValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static StringValues GetCacheControlHeader(TimeSpan? cacheLifetime)
    {
        CacheControlHeaderValue header;

        if (cacheLifetime is { } maxAge)
        {
            header = new()
            {
                MaxAge = maxAge,
                Private = true,
            };
        }
        else
        {
            header = new()
            {
                NoCache = true,
                NoStore = true,
            };
        }

        return new StringValues(header.ToString());
    }

    private string GetContentType(ResourceEntry entry)
        => entry.ContentType ??
           (_contentTypeProvider.TryGetContentType(entry.ResourceName, out var contentType)
               ? contentType
               : "application/octet-stream");

    private (string ETag, bool Compressed) GetContentProperties(ResourceEntry entry)
    {
        if (entry.ETag == null)
        {
            using var compressed = GetResource(entry);
            using var decompressed = new MemoryStream((int)compressed.Length * 2);
            using var gzip = new GZipStream(compressed, CompressionMode.Decompress);

            gzip.CopyTo(decompressed);

            compressed.Seek(0, SeekOrigin.Begin);
            decompressed.Seek(0, SeekOrigin.Begin);

            // Some embedded resources may already be compressed or compress worse than the original
            entry.SupportsCompression = compressed.Length < decompressed.Length;

            var content = entry.SupportsCompression
                ? compressed
                : decompressed;

            var hash = SHA1.HashData(content);

            entry.CompressedLength = compressed.Length;
            entry.DecompressedLength = decompressed.Length;
            entry.ETag = $"\"{Convert.ToBase64String(hash)}\"";
        }

        return (entry.ETag, entry.SupportsCompression);
    }

    private Stream GetResource(ResourceEntry entry)
        => _assembly.GetManifestResourceStream(entry.ResourceName)!;

    private sealed class ResourceEntry(string resourceName)
    {
        public long? CompressedLength { get; set; }

        public string? ContentType { get; set; }

        public long? DecompressedLength { get; set; }

        public string? ETag { get; set; }

        public string ResourceName { get; } = resourceName;

        public bool SupportsCompression { get; set; }
    }
}
