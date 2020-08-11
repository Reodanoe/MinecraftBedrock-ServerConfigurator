using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace BedrockServerConfigurator.Library
{
    public static class Utilities
    {
        /// <summary>
        /// Random number generator used in library
        /// </summary>
        public static Random RandomGenerator = new Random();

        /// <summary>
        /// Runs a command on the running machine.
        /// </summary>
        /// <param name="windows"></param>
        /// <param name="ubuntu"></param>
        /// <returns></returns>
        public static Process RunShellCommand(string windows, string ubuntu)
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

        /// <summary>
        /// Returns random TimeSpan set between 2 TimeSpans
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static TimeSpan RandomTimeSpan(TimeSpan min, TimeSpan max)
        {
            if (max < min)
            {
                throw new ArgumentException("maxDelay has to be bigger than minDelay", "maxDelay");
            }

            var timeBetween = max - min;

            if (timeBetween.TotalMilliseconds > int.MaxValue)
            {
                throw new OverflowException("Difference between TimeSpans is larger than TimeSpan.FromMilliseconds(int.MaxValue)");
            }

            var randomDelayMiliseconds = RandomGenerator.Next((int)timeBetween.TotalMilliseconds);
            var randomDelay = min.TotalMilliseconds + randomDelayMiliseconds;

            return TimeSpan.FromMilliseconds(randomDelay);
        }

        /// <summary>
        /// Runs a shell commands that kills all Minecraft servers
        /// </summary>
        public static void KillRunningMinecraftServers()
        {
            RunShellCommand(
                windows: "taskkill /f /fi \"imagename eq bedrock_server.exe\"",
                ubuntu: "killall bedrock_server").Start();
        }

        /// <summary>
        /// Replaces . in decimal string with current culture's decimal separator
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string DecimalStringToCurrentCulture(string num)
        {
            return num.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        /// <summary>
        /// Replaces current culture's decimal separator in decimal string with .
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string DecimalStringToDot(string num)
        {
            return num.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
        }
    }
}
