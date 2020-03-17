using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text.RegularExpressions;

namespace MinecraftBedrockServerConfigurator
{
    class Configurator
    {
        /// <summary>
        /// Folder where all servers reside
        /// </summary>
        public string ServersRootPath { get; }

        /// <summary>
        /// Name for each server
        /// </summary>
        public string ServerName { get; }

        /// <summary>
        /// Returns a path to the template server
        /// </summary>
        public string OriginalServerFolderPath => Path.Combine(ServersRootPath, ServerName);

        /// <summary>
        /// Holds all servers (except template one) that are in ServersRootPath directory
        /// </summary>
        public List<Server> AllServers { get; private set; } = new List<Server>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serversRootPath">Path to a folder where all servers will reside. If it doesn't exist it will create it.</param>
        /// <param name="serverName">Name of individual servers.</param>
        public Configurator(string serversRootPath, string serverName)
        {
            AppDomain.CurrentDomain.ProcessExit += (a, b) => StopAllServers();
            Console.CancelKeyPress += (a, b) =>
            {
                StopAllServers();
                Environment.Exit(0);
            };

            ServersRootPath = serversRootPath;
            ServerName = serverName;

            Directory.CreateDirectory(OriginalServerFolderPath);

            if (serverName.Contains("_"))
            {
                throw new Exception("Dont use _ in serverName");
            }
        }

        /// <summary>
        /// Downloads server that will be used as a template for creating new servers
        /// </summary>
        public void DownloadBedrockServer()
        {
            string zipFilePath = Path.Combine(OriginalServerFolderPath, ServerName + ".zip");

            using var client = new WebClient();

            Console.WriteLine("Download started...");
            client.DownloadFile(GetUrl(client), zipFilePath);

            Console.WriteLine("Unzipping...");
            ZipFile.ExtractToDirectory(zipFilePath, OriginalServerFolderPath);

            Console.WriteLine("Deleting zip file...");
            File.Delete(zipFilePath);

            Console.WriteLine("Download finished");
        }

        /// <summary>
        /// Copies server template into a new folder which makes it a new server
        /// </summary>
        public void NewServer()
        {
            Console.WriteLine("Creating new server");

            var newServerPath = Path.Combine(ServersRootPath, ServerName + NewServerNumber());

            Console.WriteLine("Original = " + OriginalServerFolderPath);
            Console.WriteLine("New = " + newServerPath);

            var copyFolder = Utilities.RunACommand(
                             windows: $"xcopy /E /I \"{OriginalServerFolderPath}\" \"{newServerPath}\"",
                             ubuntu: $"cp -r \"{OriginalServerFolderPath}\" \"{newServerPath}\"");

            copyFolder.Start();

            _ = copyFolder.StandardOutput.ReadToEnd();

            Console.WriteLine("Folder copied");
        }

        /// <summary>
        /// Instantiates and adds all servers to AllServers
        /// </summary>
        public void LoadServers()
        {
            foreach (var name in AllServerDirectories())
            {
                var serverFolder = Path.Combine(ServersRootPath, name);

                var instance = Utilities.RunACommand(
                               windows: $"cd {serverFolder} && bedrock_server.exe",
                               ubuntu: $"cd {serverFolder} && chmod +x bedrock_server && ./bedrock_server");

                var properties = GenerateProperties(File.ReadAllText(Path.Combine(serverFolder, "server.properties")));

                AllServers.Add(new Server(instance, name, serverFolder, properties));
            }

            FixServerProperties();
        }

        /// <summary>
        /// Runs a command on specified server
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="command"></param>
        public void RunCommandOnSpecifiedServer(string serverName, string command)
        {
            if(AllServers.Any(x => x.Name == serverName))
            {
                AllServers.First(x => x.Name == serverName).RunACommand(command);
            }
            else
            {
                Console.WriteLine($"Couldn't run command \"{command}\" because server \"{serverName}\" doesn't exist.");
            }
        }

        /// <summary>
        /// Starts all servers, throws an exception if there are no servers initialized in AllServers
        /// </summary>
        public void StartAllServers()
        {
            if (!AllServers.Any())
            {
                throw new Exception("No servers are loaded. Did you forget to call LoadServers?");
            }

            AllServers.ForEach(x => x.StartServer());
        }

        /// <summary>
        /// Stop all servers in AllServers
        /// </summary>
        public void StopAllServers()
        {
            AllServers.ForEach(x => x.StopServer());
        }

        /// <summary>
        /// Gets name of all created servers (except template server)
        /// </summary>
        /// <returns></returns>
        public string[] AllServerDirectories()
        {
            return Directory.GetDirectories(ServersRootPath)
                            .Select(x => x.Split(Path.DirectorySeparatorChar)[^1])
                            .Where(y => y.Contains("_")).ToArray();
        }

        /// <summary>
        /// File server.properties but converted to dictionary
        /// </summary>
        /// <param name="propertiesString"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GenerateProperties(string propertiesString)
        {
            var lines = propertiesString.Split("\n");

            var properties = new Dictionary<string, string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var currentLine = lines[i];

                if (currentLine.StartsWith("#") || currentLine.Length < 3)
                {
                    continue;
                }

                var propertyValue = currentLine.Split("=");

                string property = propertyValue[0];
                string value = "";

                if (propertyValue.Length > 1)
                {
                    value = propertyValue[1];
                }

                properties.Add(property, value.Trim());
            }

            return properties;
        }

        /// <summary>
        /// Gets url to download minecraft server
        /// </summary>
        /// <returns></returns>
        private string GetUrl(WebClient client)
        {
            string pattern;
            string text = client.DownloadString("https://www.minecraft.net/en-us/download/server/bedrock/");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pattern = "https://minecraft.azureedge.net/bin-win/bedrock-server-[0-9.]*.zip";
            }
            else
            {
                pattern = "https://minecraft.azureedge.net/bin-linux/bedrock-server-[0-9.]*.zip";
            }

            var reg = new Regex(pattern);
            var match = reg.Match(text);

            return match.Value;
        }

        /// <summary>
        /// Returns new highest number from all created servers
        /// </summary>
        /// <returns></returns>
        private string NewServerNumber()
        {
            var nums = AllServerDirectories().Select(name => int.Parse(name.Split("_")[^1]));

            if (nums.Any())
            {
                return $"_{nums.Max() + 1}";
            }
            else
            {
                return "_0";
            }
        }

        /// <summary>
        /// Mainly fixes ports of new servers
        /// </summary>
        private void FixServerProperties()
        {
            // gets all servers that have the same ports
            var serversWithSamePorts = AllServers
                .Where(x =>
                AllServers.Any(
                    y => ((x.ServerProperties["server-port"] == y.ServerProperties["server-port"] ||
                         x.ServerProperties["server-portv6"] == y.ServerProperties["server-portv6"]) &&
                         x.Name != y.Name)))
                .ToList();

            // removes first server (0) from servers with same ports
            serversWithSamePorts.RemoveAll(x => x.Number == 0);

            // gets all servers except those who have same ports
            var alrightServers = AllServers.Except(serversWithSamePorts).ToList();

            // adds to list with alright servers new server that have changed ports
            foreach (var server in serversWithSamePorts)
            {
                server.ServerProperties["server-port"] = $"{int.Parse(alrightServers.Last().ServerProperties["server-port"]) + 2}";
                server.ServerProperties["server-portv6"] = $"{int.Parse(alrightServers.Last().ServerProperties["server-portv6"]) + 2}";

                alrightServers.Add(server);
            }

            // final changes and updating properties
            foreach (var server in AllServers)
            {
                server.ServerProperties["max-threads"] = "2";
                server.ServerProperties["view-distance"] = "24";

                server.UpdateProperties();
            }
        }
    }
}
