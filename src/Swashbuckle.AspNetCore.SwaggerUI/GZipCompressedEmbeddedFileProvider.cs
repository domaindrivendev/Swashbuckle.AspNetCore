using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Swashbuckle.AspNetCore.SwaggerUI;

/// <summary>
/// a <see cref="EmbeddedFileProvider"/> wrapper to provider gzip decompressed resource file info
/// </summary>
/// <param name="assembly"></param>
/// <param name="baseNamespace"></param>
internal sealed class GZipCompressedEmbeddedFileProvider(Assembly assembly, string baseNamespace) : IFileProvider
{
    private readonly string _baseNamespace = string.IsNullOrEmpty(baseNamespace) ? string.Empty : baseNamespace + ".";

    private readonly EmbeddedFileProvider _embeddedFileProvider = new(assembly, baseNamespace);

    private readonly ConcurrentDictionary<string, long> _subpathLengthCache = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        //logic same as EmbeddedFileProvider
        return subpath?.Length == 0 || string.Equals(subpath, "/", StringComparison.Ordinal)
               ? new DecompresDirectoryContents(EnumerateItems().ToList())
               : new DecompresDirectoryContents([]);
    }

    /// <inheritdoc/>
    public IFileInfo GetFileInfo(string subpath) => new GZipDecompresFileInfoWrapper(_embeddedFileProvider.GetFileInfo(subpath), subpath, GetSubpathDecompressedLength);

    /// <inheritdoc/>
    public IChangeToken Watch(string filter) => _embeddedFileProvider.Watch(filter);

    private static string NormalizeSubPath(string subpath) => string.Join(".", subpath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).Where(m => !string.IsNullOrWhiteSpace(m)));

    /// <summary>
    /// Get the <paramref name="stream"/>'s length by decompress and read it
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private static long ReadStreamDecompressedLength(Stream stream)
    {
        const int BufferSize = 4096;

        long length = 0;
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        try
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true);
            var readLength = 0;
            do
            {
                readLength = gzipStream.Read(buffer, 0, BufferSize);
                length += readLength;
            } while (readLength != 0);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        return length;
    }

    private IEnumerable<IFileInfo> EnumerateItems()
    {
        return assembly.GetManifestResourceNames()
                       .Where(name => name.StartsWith(_baseNamespace))
                       .Select(name => name.Substring(_baseNamespace.Length))
                       .Select(subpath => this.GetFileInfo(subpath))
                       .Where(static fileInfo => fileInfo.Exists);
    }

    /// <summary>
    /// Get <paramref name="subpath"/>'s decompressed length and cache it
    /// </summary>
    /// <param name="subpath"></param>
    /// <returns></returns>
    private long GetSubpathDecompressedLength(string subpath)
    {
        if (_subpathLengthCache.TryGetValue(subpath, out var length))
        {
            return length;
        }

        var resourceName = $"{_baseNamespace}{NormalizeSubPath(subpath)}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return -1;
        }

        return _subpathLengthCache.GetOrAdd(subpath, _ => ReadStreamDecompressedLength(stream));
    }

    private sealed class DecompresDirectoryContents(List<IFileInfo> fileInfos) : IDirectoryContents
    {
        /// <inheritdoc/>
        public bool Exists => fileInfos.Count > 0;

        /// <inheritdoc/>
        public IEnumerator<IFileInfo> GetEnumerator() => fileInfos.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class GZipDecompresFileInfoWrapper(IFileInfo fileInfo, string subpath, Func<string, long> getSubpathLengthFunc) : IFileInfo
    {
        /// <inheritdoc/>
        public bool Exists => fileInfo.Exists;

        /// <inheritdoc/>
        public bool IsDirectory => fileInfo.IsDirectory;

        /// <inheritdoc/>
        public DateTimeOffset LastModified => fileInfo.LastModified;

        /// <inheritdoc/>
        public long Length => fileInfo.Exists ? getSubpathLengthFunc(subpath) : -1;

        /// <inheritdoc/>
        public string Name => fileInfo.Name;

        /// <inheritdoc/>
        public string PhysicalPath => fileInfo.PhysicalPath;

        /// <inheritdoc/>
        public Stream CreateReadStream()
        {
            return fileInfo.CreateReadStream() is { } stream
                   ? new GZipStream(stream, CompressionMode.Decompress)
                   : throw new FileNotFoundException(message: null, fileName: fileInfo.Name);
        }
    }
}
