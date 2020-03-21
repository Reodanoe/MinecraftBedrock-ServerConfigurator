﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text.RegularExpressions;

namespace BedrockServerConfigurator
{
    public class Configurator
    {
        // TODO
        // implement world backups

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
        /// string is the name of the server
        /// </summary>
        public Dictionary<int, Server> AllServers { get; } = new Dictionary<int, Server>();

        /// <summary>
        /// Gets all servers from AllSevers dictionary
        /// </summary>
        public List<Server> AllServersList => AllServers.Values.ToList();

        /// <summary>
        /// Saves the regex for getting a download url for minecraft bedrock server
        /// </summary>
        private Regex urlRegex;

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
            if (Directory.GetFiles(OriginalServerFolderPath).Any())
            {
                throw new Exception($"Template server already exists, delete folder \"{ServerName}\" in \"{ServersRootPath}\".");
            }

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

            var newServerPath = Path.Combine(ServersRootPath, ServerName + NewServerID());

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

                var server = new Server(instance, name, serverFolder, properties);

                AllServers.Add(server.ID, server);

                Console.WriteLine($"Loaded {name}");
            }

            FixServerProperties();
        }

        /// <summary>
        /// Runs a command on specified server
        /// </summary>
        /// <param name="serverID"></param>
        /// <param name="command"></param>
        public void RunCommandOnSpecifiedServer(int serverID, string command)
        {
            if (AllServers.TryGetValue(serverID, out Server server))
            {
                server.RunACommand(command);
            }
            else
            {
                Console.WriteLine($"Couldn't run command \"{command}\" because server with the ID \"{serverID}\" doesn't exist.");
            }
        }

        /// <summary>
        /// Starts all servers
        /// </summary>
        public void StartAllServers()
        {
            AllServersAction(x => x.StartServer());
        }

        /// <summary>
        /// Stops all servers
        /// </summary>
        public void StopAllServers()
        {
            AllServersAction(x => x.StopServer());
        }

        /// <summary>
        /// Restarts all servers
        /// </summary>
        public void RestartAllServers()
        {
            AllServersAction(x => x.RestartServer());
        }

        /// <summary>
        /// Invokes an action on all loaded servers
        /// </summary>
        /// <param name="action"></param>
        public void AllServersAction(Action<Server> action)
        {
            AllServersList.ForEach(action);
        }

        /// <summary>
        /// Gets name of all created servers (except template server)
        /// </summary>
        /// <returns></returns>
        public string[] AllServerDirectories()
        {
            return Directory
                .GetDirectories(ServersRootPath)
                .Select(x => x.Split(Path.DirectorySeparatorChar)[^1])
                .Where(y => y.Contains("_")).ToArray();
        }

        /// <summary>
        /// File server.properties but cleaned and converted to dictionary
        /// </summary>
        /// <param name="propertiesString"></param>
        /// <returns></returns>
        private Dictionary<string, string> GenerateProperties(string propertiesString)
        {
            return propertiesString
                   .Split("\n")
                   .Select(a => a.Trim())
                   .Where(b => !b.StartsWith("#") && b.Length > 0)
                   .Select(c => c.Split("="))
                   .ToDictionary(d => d[0], d => d[1]);
        }

        /// <summary>
        /// Gets url to download minecraft server
        /// </summary>
        /// <returns></returns>
        private string GetUrl(WebClient client)
        {
            if (urlRegex == null)
            {
                var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "linux";
                var pattern = $"https://minecraft.azureedge.net/bin-{os}/bedrock-server-[0-9.]*.zip";

                urlRegex = new Regex(pattern);
            }

            string text = client.DownloadString("https://www.minecraft.net/en-us/download/server/bedrock/");
            return urlRegex.Match(text).Value;
        }

        /// <summary>
        /// Returns new highest ID from all created servers
        /// </summary>
        /// <returns></returns>
        private string NewServerID()
        {
            var nums = AllServerDirectories().Select(name => int.Parse(name.Split("_")[^1]));

            if (nums.Any())
            {
                return $"_{nums.Max() + 1}";
            }
            else
            {
                return "_1";
            }
        }

        /// <summary>
        /// Mainly fixes ports of new servers
        /// </summary>
        private void FixServerProperties()
        {
            // gets all servers that have the same ports
            var serversWithSamePorts = AllServers.Values
                .Where(x =>
                AllServers.Values.Any(
                    y => ((x.ServerProperties["server-port"] == y.ServerProperties["server-port"] ||
                         x.ServerProperties["server-portv6"] == y.ServerProperties["server-portv6"]) &&
                         x.Name != y.Name)))
                .ToList();

            // removes first server (ID 1) from servers with same ports
            serversWithSamePorts.RemoveAll(x => x.ID == 1);

            // gets all servers except those who have same ports
            var alrightServers = AllServersList.Except(serversWithSamePorts);

            // adds to list with alright servers new server that have changed ports
            foreach (var server in serversWithSamePorts)
            {
                server.ServerProperties["server-port"] = $"{int.Parse(alrightServers.Last().ServerProperties["server-port"]) + 2}";
                server.ServerProperties["server-portv6"] = $"{int.Parse(alrightServers.Last().ServerProperties["server-portv6"]) + 2}";

                alrightServers.Append(server);
            }

            // final changes and updating properties
            foreach (var server in AllServersList)
            {
                server.ServerProperties["max-threads"] = "2";
                server.ServerProperties["view-distance"] = "24";

                server.UpdateProperties();
            }
        }
    }
}