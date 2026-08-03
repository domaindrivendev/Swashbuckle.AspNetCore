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
    private static readonly StringValues VaryAcceptEncodingHeader = new(HeaderNames.AcceptEncoding);

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
        if (!TryGetResourcePath(httpContext.Request.Path.Value, out var resourcePath) ||
            !_resourceCache.TryGetValue(resourcePath, out var cacheEntry))
        {
            return false;
        }

        var contentType = GetContentType(cacheEntry);
        var content = GetContent(cacheEntry);

        // The representation is selected before the conditional request is evaluated: the entity tag
        // has to identify the representation that would actually be served, otherwise a client or a
        // cache holding one representation is told that a different one has not been modified.
        var serveCompressed = content.SupportsCompression && IsGZipAccepted(httpContext.Request);

        var etag = serveCompressed ? content.CompressedETag : content.DecompressedETag;
        var body = serveCompressed ? content.Compressed : content.Decompressed;

        var response = httpContext.Response;
        var responseHeaders = response.Headers;

        // The response is negotiated on Accept-Encoding, so a cache must not share it between
        // clients that sent different values for that header.
        responseHeaders.Vary = VaryAcceptEncodingHeader;
        responseHeaders.ETag = etag;

        if (httpContext.Request.Headers.IfNoneMatch == etag)
        {
            response.StatusCode = StatusCodes.Status304NotModified;
            return true;
        }

        if (serveCompressed)
        {
            responseHeaders.ContentEncoding = GZipEncodingHeader;
        }

        responseHeaders.CacheControl = _cacheControl;
        responseHeaders.ContentLength = body.Length;
        responseHeaders.ContentType = contentType;

        await response.BodyWriter.WriteAsync(body, httpContext.RequestAborted);

        return true;
    }

    private bool TryGetResourcePath(string? requestPath, out string resourcePath)
    {
        resourcePath = string.Empty;

        var path = requestPath ?? string.Empty;

        if (!path.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = path[_pathPrefix.Length..];

        // The prefix has to be followed by a segment boundary, otherwise a path such as
        // "/swaggerXfoo.css" is treated as though it were below the configured route prefix.
        if (relativePath.Length is 0 || relativePath[0] is not '/')
        {
            return false;
        }

        resourcePath = relativePath.Replace('/', '.');

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

    private ResourceContent GetContent(ResourceEntry entry)
    {
        // Published as a single reference so that a concurrent reader either sees nothing at all or
        // sees a fully initialized value, rather than a partially populated set of fields.
        if (Volatile.Read(ref entry.Content) is { } existing)
        {
            return existing;
        }

        using var compressed = GetResource(entry);
        using var decompressed = new MemoryStream((int)compressed.Length * 2);

        using (var gzip = new GZipStream(compressed, CompressionMode.Decompress, leaveOpen: true))
        {
            gzip.CopyTo(decompressed);
        }

        var decompressedBytes = decompressed.ToArray();

        // Some embedded resources may already be compressed or compress worse than the original
        var supportsCompression = compressed.Length < decompressedBytes.Length;

        byte[]? compressedBytes = null;
        string? compressedETag = null;

        if (supportsCompression)
        {
            compressed.Seek(0, SeekOrigin.Begin);

            using var memoryStream = new MemoryStream((int)compressed.Length);
            compressed.CopyTo(memoryStream);

            compressedBytes = memoryStream.ToArray();
            compressedETag = ComputeETag(compressedBytes);
        }

        var content = new ResourceContent(
            decompressedBytes,
            compressedBytes,
            ComputeETag(decompressedBytes),
            compressedETag,
            supportsCompression);

        Volatile.Write(ref entry.Content, content);

        return content;

        static string ComputeETag(byte[] content) => $"\"{Convert.ToBase64String(SHA1.HashData(content))}\"";
    }

    private Stream GetResource(ResourceEntry entry)
        => _assembly.GetManifestResourceStream(entry.ResourceName)!;

    private sealed class ResourceEntry(string resourceName)
    {
        public ResourceContent? Content;

        public string? ContentType { get; set; }

        public string ResourceName { get; } = resourceName;
    }

    private sealed class ResourceContent(
        byte[] decompressed,
        byte[]? compressed,
        string decompressedETag,
        string? compressedETag,
        bool supportsCompression)
    {
        public byte[] Decompressed { get; } = decompressed;

        public byte[] Compressed { get; } = compressed ?? decompressed;

        public string DecompressedETag { get; } = decompressedETag;

        public string CompressedETag { get; } = compressedETag ?? decompressedETag;

        public bool SupportsCompression { get; } = supportsCompression;
    }
}
