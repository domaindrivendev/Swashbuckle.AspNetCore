using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerUI.Test;

/// <summary>
/// For <see cref="GZipCompressedEmbeddedFileProvider"/> testing
/// </summary>
public class GZipCompressedEmbeddedFileProviderTests
{
    [Theory]
    [InlineData("Swashbuckle.AspNetCore.SwaggerUI.node_modules")]
    [InlineData("Swashbuckle.AspNetCore.SwaggerUI.node_modules.swagger_ui_dist")]
    public void ResourceRead_CompressedEmbeddedFileProvider(string baseNamespace)
    {
        //confirm that GZipCompressedEmbeddedFileProvider is the same as EmbeddedFileProvider
        var provider = new EmbeddedFileProvider(typeof(SwaggerUIOptions).Assembly, baseNamespace);
        var compressedProvider = new GZipCompressedEmbeddedFileProvider(typeof(SwaggerUIOptions).Assembly, baseNamespace);
        var checkSubpaths = new string[]
        {
            "/",
            null,
            string.Empty,
            " ",
            "\t",
            "\n",
            "/swagger_ui_dist",
            "swagger_ui_dist",
            "/nodir",
            "nodir"
        };

        foreach (var subpath in checkSubpaths)
        {
            AssertResources(provider, compressedProvider, subpath);
        }

        var nonExistentFile = Guid.NewGuid().ToString();
        AssertFileInfo(provider.GetFileInfo(nonExistentFile), compressedProvider.GetFileInfo(nonExistentFile));

        static void AssertResources(IFileProvider expectedProvider, IFileProvider actualProvider, string subpath)
        {
            var expectedContents = expectedProvider.GetDirectoryContents(subpath);
            var actualContents = actualProvider.GetDirectoryContents(subpath);

            Assert.Equal(expectedContents.Exists, actualContents.Exists);
            Assert.Equal(expectedContents.Count(), actualContents.Count());
            var actualResourceMap = actualContents.ToDictionary(m => m.Name);

            foreach (var expectedFileInfo in expectedContents)
            {
                Assert.True(actualResourceMap.TryGetValue(expectedFileInfo.Name, out var actualFileInfo));
                Assert.NotNull(actualFileInfo);
                Assert.True(actualFileInfo.Exists);
                AssertFileInfo(expectedFileInfo, actualFileInfo);
                AssertFileInfo(expectedProvider.GetFileInfo(expectedFileInfo.Name), actualProvider.GetFileInfo(expectedFileInfo.Name));
            }
        }

        static void AssertFileInfo(IFileInfo expectedFileInfo, IFileInfo actualFileInfo)
        {
            Assert.Equal(expectedFileInfo.Exists, actualFileInfo.Exists);
            Assert.Equal(expectedFileInfo.IsDirectory, actualFileInfo.IsDirectory);
            Assert.Equal(expectedFileInfo.LastModified, actualFileInfo.LastModified);
            Assert.Equal(expectedFileInfo.PhysicalPath, actualFileInfo.PhysicalPath);

            if (expectedFileInfo.Exists && !expectedFileInfo.IsDirectory)
            {
                Assert.True(actualFileInfo.Length > 0);

                using var stream = actualFileInfo.CreateReadStream();

                var diskFileName = expectedFileInfo.Name.StartsWith("swagger_ui_dist.")
                                   ? expectedFileInfo.Name.Substring("swagger_ui_dist.".Length)
                                   : expectedFileInfo.Name;
                var diskFile = Path.Combine("swagger-ui-dist", diskFileName);

                Assert.True(File.Exists(diskFile));
                using var diskFileStream = File.OpenRead(diskFile);

                Assert.Equal(MD5.HashData(diskFileStream), MD5.HashData(stream));
            }
            else
            {
                Assert.Equal(expectedFileInfo.Length, actualFileInfo.Length);
                Assert.ThrowsAny<FileNotFoundException>(() => expectedFileInfo.CreateReadStream());
                Assert.ThrowsAny<FileNotFoundException>(() => actualFileInfo.CreateReadStream());
            }
        }
    }
}
