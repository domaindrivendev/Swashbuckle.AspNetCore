using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

internal sealed class DummyHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }
    public string ApplicationName { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; }
}
