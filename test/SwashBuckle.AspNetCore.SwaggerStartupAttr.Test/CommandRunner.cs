using System.Diagnostics;

namespace SwashBuckle.AspNetCore.SwaggerStartupAttr.Test
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

                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                }
            };

            process.Start();
            process.WaitForExit();
            string result = process.StandardOutput.ReadToEnd();

            return result;
        }
    }
}
