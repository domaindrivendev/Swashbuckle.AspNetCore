using Xunit;

namespace Swashbuckle.AspNetCore.Cli.Test;

[Collection(nameof(ConsoleErrorCollection))]
public sealed class ConsoleErrorTests
{
    [Fact]
    public async Task Main_Internal_ToFile_Writes_Error_To_ConsoleError_And_Returns_One()
    {
        var originalError = Console.Error;
        using var errorWriter = new StringWriter();
        Console.SetError(errorWriter);

        try
        {
            string[] args =
            [
                "_tofile",
                Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
                "nonexistent"
            ];

            int exitCode = await Program.Main(args);

            Assert.Equal(1, exitCode);
            Assert.Contains("nonexistent", errorWriter.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    [Fact]
    public async Task Main_Internal_List_Writes_Error_To_ConsoleError_And_Returns_One()
    {
        var originalError = Console.Error;
        using var errorWriter = new StringWriter();
        Console.SetError(errorWriter);

        try
        {
            string[] args =
            [
                "_list",
                Path.Combine(Directory.GetCurrentDirectory(), "does_not_exist.dll")
            ];

            int exitCode = await Program.Main(args);

            Assert.Equal(1, exitCode);
            Assert.Contains("does_not_exist.dll", errorWriter.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }
}
