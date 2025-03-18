namespace Swashbuckle.AspNetCore.TestSupport.Utilities;

public sealed class TemporaryDirectory : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateDirectory(System.IO.Path.Join(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName()));

    public string Path => _directory.FullName;

    public void Dispose()
    {
        try
        {
            _directory.Delete(recursive: true);
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    public bool Exists() => _directory.Exists;
}
