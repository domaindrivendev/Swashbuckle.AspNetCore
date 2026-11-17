using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Swashbuckle.AspNetCore.TestSupport.Utilities;
using Xunit;

namespace Swashbuckle.AspNetCore.Cli.Test;

public static class ToolTests
{
    [Fact]
    public static async Task Can_Output_Swagger_Document_Names()
    {
        var result = await RunToStringCommandAsync((outputPath) =>
        [
            "list",
            "--output",
            outputPath,
            Path.Combine(Directory.GetCurrentDirectory(), "MultipleVersions.dll")
        ], nameof(Can_Output_Swagger_Document_Names));
        var expected = $"\"1.0\"{Environment.NewLine}\"2.0\"{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public static async Task Throws_When_Startup_Assembly_Does_Not_Exist()
    {
        string[] args = ["tofile", "--output", "swagger.json", "--openapiversion", "2.0", "./does_not_exist.dll", "v1"];
        await Assert.ThrowsAsync<FileNotFoundException>(() => Program.Main(args));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("1.9")]
    public static async Task Error_When_OpenApiVersion_Is_Not_Supported(string version)
    {
        string[] args =
        [
            "tofile",
            "--output",
            "swagger.json",
            "--openapiversion",
            version,
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1"
        ];
        Assert.NotEqual(0, await Program.Main(args));
    }

    [Fact]
    public static async Task Can_Generate_Swagger_Json_v2_OpenApiVersion()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "2.0",
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1"
        ]);

        Assert.Equal("2.0", document.RootElement.GetProperty("swagger").GetString());

        // verify one of the endpoints
        var paths = document.RootElement.GetProperty("paths");
        var productsPath = paths.GetProperty("/products");
        Assert.True(productsPath.TryGetProperty("post", out _));
    }

    [Fact]
    public static async Task Can_Generate_Swagger_Json_v3()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1"
        ]);

        Assert.StartsWith("3.0.", document.RootElement.GetProperty("openapi").GetString());

        // verify one of the endpoints
        var paths = document.RootElement.GetProperty("paths");
        var productsPath = paths.GetProperty("/products");
        Assert.True(productsPath.TryGetProperty("post", out _));
    }

    [Fact]
    public static async Task Can_Generate_Swagger_Json_v3_1()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "3.1",
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1"
        ]);

        Assert.StartsWith("3.1.", document.RootElement.GetProperty("openapi").GetString());

        // verify one of the endpoints
        var paths = document.RootElement.GetProperty("paths");
        var productsPath = paths.GetProperty("/products");
        Assert.True(productsPath.TryGetProperty("post", out _));
    }

    [Fact]
    public static async Task Can_Generate_Swagger_Json_v3_2()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "3.2",
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1"
        ]);

        Assert.StartsWith("3.2.", document.RootElement.GetProperty("openapi").GetString());

        // verify one of the endpoints
        var paths = document.RootElement.GetProperty("paths");
        var productsPath = paths.GetProperty("/products");
        Assert.True(productsPath.TryGetProperty("post", out _));
    }

    [Fact]
    public static async Task Can_Generate_Swagger_Json_v3_OpenApiVersion()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "3.0",
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1"
        ]);

        Assert.StartsWith("3.0.", document.RootElement.GetProperty("openapi").GetString());

        // verify one of the endpoints
        var paths = document.RootElement.GetProperty("paths");
        var productsPath = paths.GetProperty("/products");
        Assert.True(productsPath.TryGetProperty("post", out _));
    }

    [Fact]
    public static async Task Can_Generate_Swagger_Json_With_Async_Operation_Filters()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            Path.Combine(Directory.GetCurrentDirectory(), "Authorization.dll"),
            "v1"
        ]);

        var paths = document.RootElement.GetProperty("paths");
        var resourcesPath = paths.GetProperty("/resources/{id}");
        var getOp = resourcesPath.GetProperty("get");

        // verify that the async operation filter resolved authorization policies and added security requirements
        Assert.True(getOp.TryGetProperty("security", out var security));
        Assert.True(security.GetArrayLength() > 0);

        // verify that 401/403 responses were added by the async filter
        var responses = getOp.GetProperty("responses");
        Assert.True(responses.TryGetProperty("401", out _));
        Assert.True(responses.TryGetProperty("403", out _));
    }

    [Fact]
    public static async Task Overwrites_Existing_File()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        {
            File.WriteAllText(outputPath, new string('x', 100_000));

            return
            [
                "tofile",
                "--output",
                outputPath,
                "--openapiversion",
                "2.0",
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
    public static async Task CustomDocumentSerializer_Writes_Custom_V2_Document()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "2.0",
            Path.Combine(Directory.GetCurrentDirectory(), "CustomDocumentSerializer.dll"),
            "v1"
        ]);

        // verify that the custom serializer wrote the swagger info
        var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
        Assert.Equal("DocumentSerializerTest2.0", swaggerInfo);
    }

    [Fact]
    public static async Task CustomDocumentSerializer_Writes_Custom_V3_Document()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
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
    public static async Task CustomDocumentSerializer_Writes_Custom_V3_1_Document()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "3.1",
            Path.Combine(Directory.GetCurrentDirectory(),
            "CustomDocumentSerializer.dll"),
            "v1"
        ]);

        // verify that the custom serializer wrote the swagger info
        var swaggerInfo = document.RootElement.GetProperty("swagger").GetString();
        Assert.Equal("DocumentSerializerTest3.1", swaggerInfo);
    }

    [Fact]
    public static async Task Can_Generate_Swagger_Json_ForTopLevelApp()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "2.0",
            Path.Combine(Directory.GetCurrentDirectory(), "MinimalApp.dll"),
            "v1"
        ]);

        // verify one of the endpoints
        var paths = document.RootElement.GetProperty("paths");
        var path = paths.GetProperty("/WeatherForecast");
        Assert.True(path.TryGetProperty("get", out _));
    }

    [Fact]
    public static async Task Does_Not_Run_Crashing_HostedService()
    {
        using var document = await RunToJsonCommandAsync((outputPath) =>
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
    public static async Task Creates_New_Folder_Path()
    {
        using var document = await RunToJsonCommandAsync(outputPath =>
        [
            "tofile",
            "--output",
            outputPath,
            "--openapiversion",
            "2.0",
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1"
        ], GenerateRandomString(5));

        // verify one of the endpoints
        var paths = document.RootElement.GetProperty("paths");
        var productsPath = paths.GetProperty("/products");
        Assert.True(productsPath.TryGetProperty("post", out _));
    }

    [Fact]
    public static async Task Can_Verify_Swagger_Json_v3()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var outputPath = Path.Combine(temporaryDirectory.Path, "swagger.json");
        var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll");

        Assert.Equal(0, await Program.Main(
        [
            "tofile",
            "--output",
            outputPath,
            assemblyPath,
            "v1"
        ]));

        var expectedLastModified = File.GetLastWriteTimeUtc(outputPath);

        Assert.Equal(0, await Program.Main(
        [
            "verify",
            "--input",
            outputPath,
            assemblyPath,
            "v1"
        ]));

        Assert.Equal(expectedLastModified, File.GetLastWriteTimeUtc(outputPath));
    }

    [Fact]
    public static async Task Can_Verify_Swagger_Yaml()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var outputPath = Path.Combine(temporaryDirectory.Path, "swagger.yaml");
        var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll");

        Assert.Equal(0, await Program.Main(
        [
            "tofile",
            "--yaml",
            "--output",
            outputPath,
            assemblyPath,
            "v1"
        ]));

        Assert.Equal(0, await Program.Main(
        [
            "verify",
            "--yaml",
            "--input",
            outputPath,
            assemblyPath,
            "v1"
        ]));
    }

    [Fact]
    public static async Task Can_Fail_Verification_When_Input_File_Is_Different()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var outputPath = Path.Combine(temporaryDirectory.Path, "swagger.json");

        File.WriteAllText(outputPath, "different-content");

        var result = await RunCliProcessAsync(
            "verify",
            "--yaml",
            "--openapiversion",
            "3.1",
            "--input",
            outputPath,
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1");

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("does not match", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("dotnet swagger tofile --output", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("--openapiversion 3.1 --yaml", result.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task Can_Fail_Verification_When_Input_File_Contains_Extra_Content()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var outputPath = Path.Combine(temporaryDirectory.Path, "swagger.json");
        var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll");

        Assert.Equal(0, await Program.Main(
        [
            "tofile",
            "--output",
            outputPath,
            assemblyPath,
            "v1"
        ]));

        File.AppendAllText(outputPath, "extra-content");

        var result = await RunCliProcessAsync(
            "verify",
            "--input",
            outputPath,
            assemblyPath,
            "v1");

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("does not match", result.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task Can_Fail_Verification_When_Input_File_Has_Same_Length_But_Different_Content()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var outputPath = Path.Combine(temporaryDirectory.Path, "swagger.json");
        var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll");

        Assert.Equal(0, await Program.Main(
        [
            "tofile",
            "--output",
            outputPath,
            assemblyPath,
            "v1"
        ]));

        var bytes = File.ReadAllBytes(outputPath);
        var originalLength = bytes.Length;
        var targetIndex = Array.IndexOf(bytes, (byte)'p');
        Assert.True(targetIndex >= 0);
        bytes[targetIndex] = (byte)'q';
        File.WriteAllBytes(outputPath, bytes);

        Assert.Equal(originalLength, new FileInfo(outputPath).Length);

        var result = await RunCliProcessAsync(
            "verify",
            "--input",
            outputPath,
            assemblyPath,
            "v1");

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("does not match", result.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task Can_Fail_Verification_When_Input_File_Does_Not_Exist()
    {
        var result = await RunCliProcessAsync(
            "verify",
            "--input",
            "does_not_exist.json",
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("does not exist", result.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task Can_Fail_Verification_When_Input_Option_Is_Not_Set()
    {
        var result = await RunCliProcessAsync(
            "verify",
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("--input <value>", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("(required)", result.StandardOutput, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--openapiversion", "3.1")]
    [InlineData("--host", "api.example.com")]
    [InlineData("--basepath", "/api")]
    public static async Task Can_Verify_Swagger_With_Shared_Options(string optionName, string optionValue)
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var outputPath = Path.Combine(temporaryDirectory.Path, "swagger.json");
        var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll");

        Assert.Equal(0, await Program.Main(
        [
            "tofile",
            optionName,
            optionValue,
            "--output",
            outputPath,
            assemblyPath,
            "v1"
        ]));

        Assert.Equal(0, await Program.Main(
        [
            "verify",
            optionName,
            optionValue,
            "--input",
            outputPath,
            assemblyPath,
            "v1"
        ]));

        var mismatchResult = await RunCliProcessAsync(
            "verify",
            "--input",
            outputPath,
            assemblyPath,
            "v1");

        Assert.Equal(2, mismatchResult.ExitCode);
        Assert.Contains("does not match", mismatchResult.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task Can_Fail_Verification_When_Startup_Assembly_Does_Not_Exist()
    {
        var result = await RunCliProcessAsync(
            "verify",
            "--input",
            "swagger.json",
            "./does_not_exist.dll",
            "v1");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("does_not_exist.dll", result.StandardError, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("swagger.json", false, "swagger.json")]
    [InlineData("openapi(v1).json", false, "'openapi(v1).json'")]
    [InlineData("api's.json", false, "'api'\\''s.json'")]
    [InlineData("my specs/openapi(v1).json", false, "'my specs/openapi(v1).json'")]
    [InlineData("C:\\specs\\swagger.json", true, "C:\\specs\\swagger.json")]
    [InlineData("openapi(v1).json", true, "\"openapi(v1).json\"")]
    [InlineData("api's.json", true, "\"api's.json\"")]
    public static void FormatShellArgument_QuotesMetacharacters_Appropriately(string input, bool isWindows, string expected)
    {
        Assert.Equal(expected, Program.FormatShellArgument(input, isWindows));
    }

    [Theory]
    [InlineData("openapi(v1).json")]
    [InlineData("api's.json")]
    public static async Task Can_Fail_Verification_When_Input_File_Has_Shell_Metacharacters(string fileName)
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var outputPath = Path.Combine(temporaryDirectory.Path, fileName);

        File.WriteAllText(outputPath, "different-content");

        var result = await RunCliProcessAsync(
            "verify",
            "--input",
            outputPath,
            Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
            "v1");

        Assert.Equal(2, result.ExitCode);
        Assert.Contains($"--output {Program.FormatShellArgument(outputPath)}", result.StandardError, StringComparison.Ordinal);
    }

    private static async Task<(int ExitCode, string StandardOutput, string StandardError)> RunCliProcessAsync(params string[] args)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.StartInfo.ArgumentList.Add("exec");
        process.StartInfo.ArgumentList.Add(typeof(Program).Assembly.Location);
        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        Assert.True(process.Start());

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cts.Token);
        var stderrTask = process.StandardError.ReadToEndAsync(cts.Token);

        await process.WaitForExitAsync(cts.Token);

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static async Task<string> RunToStringCommandAsync(Func<string, string[]> setup, string subOutputPath = default)
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var outputPath = !string.IsNullOrEmpty(subOutputPath)
            ? Path.Combine(temporaryDirectory.Path, subOutputPath, "swagger.json")
            : Path.Combine(temporaryDirectory.Path, "swagger.json");

        string[] args = setup(outputPath);

        Assert.Equal(0, await Program.Main(args));

        return File.ReadAllText(outputPath);
    }

    private static async Task<JsonDocument> RunToJsonCommandAsync(Func<string, string[]> setup, string subOutputPath = default)
    {
        string json = await RunToStringCommandAsync(setup, subOutputPath);
        return JsonDocument.Parse(json);
    }

    private static string GenerateRandomString(int length)
    {
        const string Choices = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(Choices[Random.Shared.Next(Choices.Length)]);
        }

        return result.ToString();
    }

}
