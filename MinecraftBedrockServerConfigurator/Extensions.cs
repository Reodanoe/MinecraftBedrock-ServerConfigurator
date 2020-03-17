using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MinecraftBedrockServerConfigurator
{
    static class Utilities
    {
        /// <summary>
        /// Runs a command on the running machine.
        /// </summary>
        /// <param name="windows"></param>
        /// <param name="ubuntu"></param>
        /// <returns></returns>
        public static Process RunACommand(string windows, string ubuntu)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C {windows}";
            }
            else
            {
                var escapedCommand = ubuntu.Replace("\"", "\\\"");

                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{escapedCommand}\"";
            }

            return process;
        }
    }
}
