using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BedrockServerConfigurator.Library.ServerFiles;
using BedrockServerConfigurator.Library.Commands;

namespace BedrockServerConfigurator.Library
{
    public class Configurator
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
        public string TemplateServerDirectoryPath => Path.Combine(ServersRootPath, ServerName);

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
        /// List of running servers
        /// </summary>
        public List<Server> RunningServers => AllServersList.Where(x => x.Running).ToList();

        /// <summary>
        /// Saves the regex for getting a download url for minecraft bedrock server
        /// </summary>
        private Regex urlRegex;

        /// <summary>
        /// Logs all messages from Configurator
        /// </summary>
        public event Action<string> Log;

        /// <summary>
        /// Returns true if folder with template server (downloaded server) has any files
        /// </summary>
        public bool TemplateServerExists => Directory.Exists(TemplateServerDirectoryPath);

        /// <summary>
        /// When downloading template server this event will give information about the download, e.g. percentage
        /// </summary>
        public event DownloadProgressChangedEventHandler TemplateServerDownloadChanged;

        /// <summary>
        /// Prevents creating more than one instance
        /// </summary>
        private static bool created;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serversRootPath">Path to a folder where all servers will reside. If it doesn't exist it will create it.</param>
        /// <param name="serverName">Name of individual servers.</param>
        public Configurator(string serversRootPath, string serverName)
        {
            if (created)
            {
                throw new Exception("Configurator is already instantiated, please use 1 instance only.");
            }

            if (serverName.Contains("_"))
            {
                throw new ArgumentException("Dont use \"_\" in serverName", "serverName");
            }

            ServersRootPath = serversRootPath;
            ServerName = serverName;

            Directory.CreateDirectory(ServersRootPath);

            created = true;
        }

        /// <summary>
        /// Downloads server that will be used as a template for creating new servers
        /// </summary>
        public async Task DownloadBedrockServer()
        {
            if (TemplateServerExists)
            {
                throw new Exception($"Template server already exists, delete folder \"{ServerName}\" in \"{ServersRootPath}\".");
            }

            Directory.CreateDirectory(TemplateServerDirectoryPath);

            string zipFilePath = Path.Combine(TemplateServerDirectoryPath, ServerName + ".zip");

            using var client = new WebClient();

            CallLog("Getting url address...");
            string url = GetDownloadUrlFromWebsite(client);
            string version = url.Split("-").Last()[..^4];

            client.DownloadProgressChanged += (_, downloadProgressChanged) => TemplateServerDownloadChanged?.Invoke(this, downloadProgressChanged);

            CallLog("Download started...");
            await client.DownloadFileTaskAsync(new Uri(url), zipFilePath);

            CallLog("Unzipping...");
            ZipFile.ExtractToDirectory(zipFilePath, TemplateServerDirectoryPath);

            CallLog("Deleting zip file...");
            File.Delete(zipFilePath);

            CallLog("Adding version file...");
            File.WriteAllText(Path.Combine(TemplateServerDirectoryPath, "version.txt"), version);

            CallLog("Download finished");
        }

        /// <summary>
        /// Copies server template into a new folder which makes it a new server
        /// </summary>
        /// <returns>Path to newly created server</returns>
        public string CreateNewServer()
        {
            CallLog("Creating new server");

            var newServerPath = Path.Combine(ServersRootPath, ServerName + NewServerID());

            CallLog("Original = " + TemplateServerDirectoryPath);
            CallLog("New = " + newServerPath);

            var copyFolder = Utilities.RunShellCommand(
                                windows: $"xcopy /E /I \"{TemplateServerDirectoryPath}\" \"{newServerPath}\"",
                                ubuntu: $"cp -r \"{TemplateServerDirectoryPath}\" \"{newServerPath}\"");

            copyFolder.Start();

            _ = copyFolder.StandardOutput.ReadToEnd();

            CallLog("Folder copied");

            return newServerPath;
        }

        /// <summary>
        /// Instantiates and adds all unloaded servers to AllServers
        /// </summary>
        public void LoadServers()
        {
            foreach (var serverFolder in AllServerDirectoriesPaths().Except(AllServers.Select(x => x.Value.FullPath)))
            {
                var server = new Server(serverFolder);

                AllServers.Add(server.ID, server);

                CallLog($"Loaded {server.Name}");
            }

            FixServerPorts();
        }

        /// <summary>
        /// Returns properties in server.properties file in template server the C# way
        /// </summary>
        /// <returns></returns>
        public string GeneratePropertiesClass()
        {
            var props = new Properties(Path.Combine(TemplateServerDirectoryPath, "server.properties"), false);
            var result = props.GenerateProperties();

            return result;
        }

        /// <summary>
        /// Stops the server, removes it from collection of AllServers and deletes directory
        /// </summary>
        /// <param name="server"></param>
        public async Task DeleteServerAsync(Server server)
        {
            await server.StopServerAsync();
            AllServers.Remove(server.ID);
            Directory.Delete(server.FullPath, true);

            CallLog($"Server {server.Name} has been deleted");
        }

        /// <summary>
        /// Runs a command on specified server
        /// </summary>
        /// <param name="serverID"></param>
        /// <param name="command"></param>
        public async Task RunCommandOnSpecifiedServerAsync(int serverId, string command)
        {
            if (AllServers.TryGetValue(serverId, out Server server))
            {
                await server.RunCommandAsync(command);
            }
            else
            {
                CallLog($"Couldn't run command \"{command}\" because server with the ID \"{serverId}\" doesn't exist.");
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
            AllServersAction(async x => await x.StopServerAsync());
        }

        /// <summary>
        /// Restarts all servers
        /// </summary>
        public void RestartAllServers()
        {
            AllServersAction(async x => await x.RestartServerAsync());
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
        /// Gets paths of all created servers inside ServersRootPath
        /// </summary>
        /// <returns></returns>
        public string[] AllServerDirectoriesPaths()
        {
            return Directory
                .GetDirectories(ServersRootPath)
                .Where(y => y.Contains("_"))
                .ToArray();
        }

        /// <summary>
        /// Gets names of all created servers inside ServersRootPath
        /// </summary>
        /// <returns></returns>
        public string[] AllServerDirectoriesNames()
        {
            return AllServerDirectoriesPaths()
                .Select(x => x.Split(Path.DirectorySeparatorChar)[^1])
                .ToArray();
        }

        /// <summary>
        /// Gets url to download minecraft server
        /// </summary>
        /// <returns></returns>
        private string GetDownloadUrlFromWebsite(WebClient client)
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
            var nums = AllServerDirectoriesNames().Select(name => int.Parse(name.Split("_")[^1]));

            return nums.Any() ? $"_{nums.Max() + 1}" : "_1";
        }

        /// <summary>
        /// Updates ports to servers so each server has own port
        /// </summary>
        private void FixServerPorts()
        {
            if (!AllServers.Any()) throw new Exception("No servers are loaded");

            // gets all servers that have the same ports
            var serversWithSamePorts = AllServers.Values
                .Where(x =>
                AllServers.Values.Any(y => (x.ServerProperties.ServerPort == y.ServerProperties.ServerPort ||
                                            x.ServerProperties.ServerPortv6 == y.ServerProperties.ServerPortv6) &&
                                            x.Name != y.Name)).ToList();

            // this fixes a bug
            // when you would start server with id higher than 1 before running server with id 1 it'd run 2 extra ports on 19132 and 19133
            // which won't allow server 1 to start
            if (AllServers.TryGetValue(1, out Server firstServer))
            {
                firstServer.ServerProperties.ServerPort = 19134;
                firstServer.ServerProperties.ServerPortv6 = 19135;
                firstServer.ServerProperties.SavePropertiesToFile();
            }

            // removes first server (ID 1) from servers with same ports
            serversWithSamePorts.RemoveAll(x => x.ID == 1);

            // gets all servers except those who have same ports
            // so this will be a list of servers which ports are alright
            var alrightServers = AllServers.Values.Except(serversWithSamePorts).ToList();

            // goes through a list of serevrs that have the same ports
            // increments ipv4 and ipv6 port by 2
            // adds the server to alright servers and saves its proeprties
            foreach (var server in serversWithSamePorts)
            {
                server.ServerProperties.ServerPort = alrightServers.Last().ServerProperties.ServerPort + 2;
                server.ServerProperties.ServerPortv6 = alrightServers.Last().ServerProperties.ServerPortv6 + 2;

                alrightServers.Add(server);

                server.ServerProperties.SavePropertiesToFile();
            }
        }

        /// <summary>
        /// Creates an api for a server based on server id
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public ServerApi GetServerApi(int serverId)
        {
            if (AllServers.TryGetValue(serverId, out Server server))
            {
                return new ServerApi(server);
            }
            else
            {
                throw new IndexOutOfRangeException($"No server with the ID [{serverId}] exists");
            }
        }

        private void CallLog(string message)
        {
            Log?.Invoke(message);
        }
    }
}
