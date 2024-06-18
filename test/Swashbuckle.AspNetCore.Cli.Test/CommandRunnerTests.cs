using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Swashbuckle.AspNetCore.Cli.Test
{
    public static class CommandRunnerTests
    {
        [Fact]
        public static void Run_ParsesArgumentsAndExecutesCommands_AccordingToConfiguredMetadata()
        {
            var receivedValues = new List<string>();
            var subject = new CommandRunner("test", "a test", new StringWriter());
            subject.SubCommand("cmd1", "", c => {
                c.Option("--opt1", "");
                c.Option("--opt2", "", true);
                c.Argument("arg1", "");
                c.OnRun((namedArgs) =>
                {
                    receivedValues.Add(namedArgs["--opt1"]);
                    receivedValues.Add(namedArgs["--opt2"]);
                    receivedValues.Add(namedArgs["arg1"]);
                    return 2;
                });
            });
            subject.SubCommand("cmd2", "", c => {
                c.Option("--opt1", "");
                c.Option("--opt2", "", true);
                c.Argument("arg1", "");
                c.OnRun((namedArgs) =>
                {
                    receivedValues.Add(namedArgs["--opt1"]);
                    receivedValues.Add(namedArgs["--opt2"]);
                    receivedValues.Add(namedArgs["arg1"]);
                    return 3;
                });
            });

            var cmd1ExitCode = subject.Run(["cmd1", "--opt1", "foo", "--opt2", "bar"]);
            var cmd2ExitCode = subject.Run(["cmd2", "--opt1", "blah", "--opt2", "dblah"]);

            Assert.Equal(2, cmd1ExitCode);
            Assert.Equal(3, cmd2ExitCode);
            Assert.Equal(["foo", null, "bar", "blah", null, "dblah"], [.. receivedValues]);
        }

        [Fact]
        public static void Run_PrintsAvailableCommands_WhenUnexpectedCommandIsProvided()
        {
            var output = new StringWriter();
            var subject = new CommandRunner("test", "a test", output);
            subject.SubCommand("cmd", "does something", c => {
            });

            var exitCode = subject.Run(["foo"]);

            Assert.StartsWith("a test", output.ToString());
            Assert.Contains("Commands:", output.ToString());
            Assert.Contains("cmd:  does something", output.ToString());
        }

        [Fact]
        public static void Run_PrintsAvailableCommands_WhenHelpOptionIsProvided()
        {
            var output = new StringWriter();
            var subject = new CommandRunner("test", "a test", output);
            subject.SubCommand("cmd", "does something", c => {
            });

            var exitCode = subject.Run(["--help"]);

            Assert.StartsWith("a test", output.ToString());
            Assert.Contains("Commands:", output.ToString());
            Assert.Contains("cmd:  does something", output.ToString());
        }

        [Theory]
        [InlineData(new[] { "--opt1" }, new string[0], new[] { "cmd", "--opt2", "foo" }, true)]
        [InlineData(new[] { "--opt1" }, new string[0], new[] { "cmd", "--opt1" }, true)]
        [InlineData(new[] { "--opt1" }, new string[0], new[] { "cmd", "--opt1", "--opt2" }, true)]
        [InlineData(new[] { "--opt1" }, new string[0], new[] { "cmd", "--opt1", "foo" }, false)]
        [InlineData(new string[0], new[] { "arg1" }, new[] { "cmd" }, true)]
        [InlineData(new string[0], new[] { "arg1" }, new[] { "cmd", "--opt1" }, true)]
        [InlineData(new string[0], new[] { "arg1" }, new[] { "cmd", "foo", "bar" }, true)]
        [InlineData(new string[0], new[] { "arg1" }, new[] { "cmd", "foo" }, false)]
        public static void Run_PrintsCommandUsage_WhenUnexpectedArgumentsAreProvided(
            string[] optionNames,
            string[] argNames,
            string[] providedArgs,
            bool shouldPrintUsage)
        {
            var output = new StringWriter();
            var subject = new CommandRunner("test", "a test", output);
            subject.SubCommand("cmd", "a command", c =>
            {
                foreach (var name in optionNames)
                    c.Option(name, "");
                foreach (var name in argNames)
                    c.Argument(name, "");
            });

            subject.Run(providedArgs);

            if (shouldPrintUsage)
                Assert.StartsWith("Usage: test cmd", output.ToString());
            else
                Assert.Empty(output.ToString());
        }
    }
}
