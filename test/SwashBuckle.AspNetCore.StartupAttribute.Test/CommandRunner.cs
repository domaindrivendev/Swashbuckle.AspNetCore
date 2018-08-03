using System.Diagnostics;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test
{
    public static class CommandRunner
    {
        public static string Run(this string args)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {args}",

                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }
    }
}
