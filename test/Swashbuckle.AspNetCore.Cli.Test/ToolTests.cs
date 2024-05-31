using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Swashbuckle.AspNetCore.TestSupport.Utilities;
using Xunit;

namespace Swashbuckle.AspNetCore.Cli.Test
{
    public static class ToolTests
    {
        [Fact]
        public static void Throws_When_Startup_Assembly_Does_Not_Exist()
        {
            string[] args = ["tofile", "--output", "swagger.json", "--serializeasv2", "./does_not_exist.dll", "v1"];
            Assert.Throws<FileNotFoundException>(() => Program.Main(args));
        }

        [Fact]
        public static void Can_Generate_Swagger_Json()
        {
            using var document = RunApplication((outputPath) =>
            [
                "tofile",
                "--output",
                outputPath,
                "--serializeasv2",
                Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
                "v1"
            ]);

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var productsPath = paths.GetProperty("/products");
            Assert.True(productsPath.TryGetProperty("post", out _));
        }

        [Fact]
        public static void Overwrites_Existing_File()
        {
            using var document = RunApplication((outputPath) =>
            {
                File.WriteAllText(outputPath, new string('x', 100_000));

                return
                [
                    "tofile",
                    "--output",
                    outputPath,
                    "--serializeasv2",
                    Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
                    "v1"
                ];
            });

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var productsPath = paths.GetProperty("/products");
            Assert.True(productsPath.TryGetProperty("post", out _));
        }

        [Fact]
        public static void CustomDocumentSerializer_Writes_Custom_V2_Document()
        {
            using var document = RunApplication((outputPath) =>
            [
                "tofile",
                "--output",
                outputPath,
                "--serializeasv2",
                Path.Combine(Directory.GetCurrentDirectory(), "CustomDocumentSerializer.dll"),
                "v1"
            ]);

            // verify that the custom serializer wrote the swagger info
            var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
            Assert.Equal("DocumentSerializerTest2.0", swaggerInfo);
        }

        [Fact]
        public static void CustomDocumentSerializer_Writes_Custom_V3_Document()
        {
            using var document = RunApplication((outputPath) =>
            [
                "tofile",
                "--output",
                outputPath,
                Path.Combine(Directory.GetCurrentDirectory(),
                "CustomDocumentSerializer.dll"),
                "v1"
            ]);

            // verify that the custom serializer wrote the swagger info
            var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
            Assert.Equal("DocumentSerializerTest3.0", swaggerInfo);
        }

        [Fact]
        public static void Can_Generate_Swagger_Json_ForTopLevelApp()
        {
            using var document = RunApplication((outputPath) =>
            [
                "tofile",
                "--output",
                outputPath,
                "--serializeasv2",
                Path.Combine(Directory.GetCurrentDirectory(), "MinimalApp.dll"),
                "v1"
            ]);

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var path = paths.GetProperty("/WeatherForecast");
            Assert.True(path.TryGetProperty("get", out _));
        }

        [Fact]
        public static void Does_Not_Run_Crashing_HostedService()
        {
            using var document = RunApplication((outputPath) =>
            [
                "tofile",
                "--output",
                outputPath,
                Path.Combine(Directory.GetCurrentDirectory(), "MinimalAppWithHostedServices.dll"),
                "v1"
            ]);

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var path = paths.GetProperty("/ShouldContain");
            Assert.True(path.TryGetProperty("get", out _));
        }

        [Fact]
        public static void Creates_New_Folder_Path()
        {
            using var document = RunApplication(outputPath =>
            [
                "tofile",
                "--output",
                outputPath,
                "--serializeasv2",
                Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
                "v1"
            ], GenerateRandomString(5));

            // verify one of the endpoints
            var paths = document.RootElement.GetProperty("paths");
            var productsPath = paths.GetProperty("/products");
            Assert.True(productsPath.TryGetProperty("post", out _));
        }

        private static JsonDocument RunApplication(Func<string, string[]> setup, string subOutputPath = default)
        {
            using var temporaryDirectory = new TemporaryDirectory();

            var outputPath = !string.IsNullOrEmpty(subOutputPath)
                ? Path.Combine(temporaryDirectory.Path, subOutputPath, "swagger.json")
                : Path.Combine(temporaryDirectory.Path, "swagger.json");

            string[] args = setup(outputPath);

            Assert.Equal(0, Program.Main(args));

            string json = File.ReadAllText(outputPath);
            return JsonDocument.Parse(json);
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

    }
}
