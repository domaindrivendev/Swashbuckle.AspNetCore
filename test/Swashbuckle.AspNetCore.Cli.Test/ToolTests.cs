using System.IO;
using System.Text.Json;
using Xunit;

namespace Swashbuckle.AspNetCore.Cli.Test
{
    public class ToolTests
    {
        [Fact]
        public void Throws_When_Startup_Assembly_Does_Not_Exist()
        {
            var args = new string[] { "tofile", "--output", "swagger.json", "--serializeasv2", "./does_not_exist.dll", "v1" };
            Assert.Throws<FileNotFoundException>(() => Program.Main(args));
        }

        [Fact]
        public void Can_Generate_Swagger_Json()
        {
            var dir = Directory.CreateDirectory(Path.Join(Path.GetTempPath(), Path.GetRandomFileName()));
            try
            {
                var args = new string[] { "tofile", "--output", $"{dir}/swagger.json", "--serializeasv2", Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"), "v1" };
                Assert.Equal(0, Program.Main(args));

                using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(dir.FullName, "swagger.json")));

                // verify one of the endpoints
                var paths = document.RootElement.GetProperty("paths");
                var productsPath = paths.GetProperty("/products");
                Assert.True(productsPath.TryGetProperty("post", out _));
            }
            finally
            {
                dir.Delete(true);
            }
        }

#if NET6_0
        [Fact]
        public void Can_Generate_Swagger_Json_ForTopLevelApp()
        {
            var dir = Directory.CreateDirectory(Path.Join(Path.GetTempPath(), Path.GetRandomFileName()));
            try
            {
                var args = new string[] { "tofile", "--output", $"{dir}/swagger.json", "--serializeasv2", Path.Combine(Directory.GetCurrentDirectory(), "MinimalApp.dll"), "v1" };
                Assert.Equal(0, Program.Main(args));

                using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(dir.FullName, "swagger.json")));

                // verify one of the endpoints
                var paths = document.RootElement.GetProperty("paths");
                var path = paths.GetProperty("/WeatherForecast");
                Assert.True(path.TryGetProperty("get", out _));
            }
            finally
            {
                dir.Delete(true);
            }
        }
#endif
    }
}
