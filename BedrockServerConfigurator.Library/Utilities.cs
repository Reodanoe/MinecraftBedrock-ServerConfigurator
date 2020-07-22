using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace BedrockServerConfigurator.Library
{
    public static class Utilities
    {
        public static Random RandomGenerator = new Random();

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

        public static DateTime GetDateTimeFromServerMessage(string message)
        {
            if (DateTime.TryParse(message[1..20], out DateTime result))
            {
                return result;
            }
            else
            {
                throw new FormatException("Cannot obtain DateTime from server message");
            }
        }

        public static TimeSpan RandomDelay(TimeSpan minDelay, TimeSpan maxDelay)
        {
            if(maxDelay < minDelay)
            {
                throw new ArgumentException("maxDelay has to be bigger than minDelay", "maxDelay");
            }

            var timeBetween = maxDelay - minDelay;
            var randomDelayMiliseconds = RandomGenerator.Next((int)timeBetween.TotalMilliseconds);
            var randomDelay = minDelay.TotalMilliseconds + randomDelayMiliseconds;

            return TimeSpan.FromMilliseconds(randomDelay);
        }
    }
}
