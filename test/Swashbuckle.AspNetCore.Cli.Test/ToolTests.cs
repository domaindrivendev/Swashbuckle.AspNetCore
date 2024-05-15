using System.IO;
using System.Text.Json;
using Swashbuckle.AspNetCore.TestSupport.Utilities;
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
            using var temporaryDirectory = new TemporaryDirectory();
            var args = new string[] { "tofile", "--output", $"{temporaryDirectory.Path}/swagger.json", "--serializeasv2", Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"), "v1" };
            Assert.Equal(0, Program.Main(args));
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(temporaryDirectory.Path, "swagger.json")));

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var productsPath = paths.GetProperty("/products");
            Assert.True(productsPath.TryGetProperty("post", out _));
        }

        [Fact]
        public void Overwrites_Existing_File()
        {
            using var temporaryDirectory = new TemporaryDirectory();
            var path = Path.Combine(temporaryDirectory.Path, "swagger.json");

            var dummyContent = new string('x', 100_000);
            File.WriteAllText(path, dummyContent);

            var args = new string[] { "tofile", "--output", path, Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"), "v1" };
            Assert.Equal(0, Program.Main(args));

            var readContent = File.ReadAllText(path);
            Assert.True(readContent.Length < dummyContent.Length);
            using var document = JsonDocument.Parse(readContent);

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var productsPath = paths.GetProperty("/products");
            Assert.True(productsPath.TryGetProperty("post", out _));
        }

        [Fact]
        public void CustomDocumentSerializer_Writes_Custom_V2_Document()
        {
            using var temporaryDirectory = new TemporaryDirectory();
            var args = new string[] { "tofile", "--output", $"{temporaryDirectory.Path}/swagger.json", "--serializeasv2", Path.Combine(Directory.GetCurrentDirectory(), "CustomDocumentSerializer.dll"), "v1" };
            Assert.Equal(0, Program.Main(args));

            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(temporaryDirectory.Path, "swagger.json")));

            // verify that the custom serializer wrote the swagger info
            var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
            Assert.Equal("DocumentSerializerTest2.0", swaggerInfo);
        }

        [Fact]
        public void CustomDocumentSerializer_Writes_Custom_V3_Document()
        {
            using var temporaryDirectory = new TemporaryDirectory();
            var args = new string[] { "tofile", "--output", $"{temporaryDirectory.Path}/swagger.json", Path.Combine(Directory.GetCurrentDirectory(), "CustomDocumentSerializer.dll"), "v1" };
            Assert.Equal(0, Program.Main(args));

            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(temporaryDirectory.Path, "swagger.json")));

            // verify that the custom serializer wrote the swagger info
            var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
            Assert.Equal("DocumentSerializerTest3.0", swaggerInfo);
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void Can_Generate_Swagger_Json_ForTopLevelApp()
        {
            using var temporaryDirectory = new TemporaryDirectory();
            var args = new string[] { "tofile", "--output", $"{temporaryDirectory.Path}/swagger.json", "--serializeasv2", Path.Combine(Directory.GetCurrentDirectory(), "MinimalApp.dll"), "v1" };
            Assert.Equal(0, Program.Main(args));

            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(temporaryDirectory.Path, "swagger.json")));

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var path = paths.GetProperty("/WeatherForecast");
            Assert.True(path.TryGetProperty("get", out _));
        }
#endif
    }
}
