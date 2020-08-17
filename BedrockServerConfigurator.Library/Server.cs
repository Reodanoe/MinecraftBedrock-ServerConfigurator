using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.ServerFiles;
using Newtonsoft.Json;
using System.Threading;

namespace BedrockServerConfigurator.Library
{
    public partial class Server
    {
        /// <summary>
        /// The process of Minecraft server
        /// </summary>
        public Process ServerInstance { get; }

        /// <summary>
        /// Name of server
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The path where server is installed
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Manipulates with server.properties file
        /// </summary>
        public Properties ServerProperties { get; }

        /// <summary>
        /// If ServerInstance is started, server is running
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// ID of a server (number at the end of the name of folder where server is located)
        /// </summary>
        public int ID => int.Parse(Name.Split("_")[^1]);

        /// <summary>
        /// Version of Minecraft server
        /// </summary>
        public string Version => File.ReadAllLines(GetFilePath("version.txt"))[0];

        /// <summary>
        /// When server started
        /// </summary>
        public DateTime ServerStartedAt { get; private set; }

        /// <summary>
        /// How long has been server running for
        /// </summary>
        public TimeSpan UpFor => DateTime.Now.Subtract(ServerStartedAt);

        /// <summary>
        /// When player connects to the server
        /// </summary>
        public event Action<ServerPlayer> OnPlayerConnected;

        /// <summary>
        /// When player disconnects from the server
        /// </summary>
        public event Action<ServerPlayer> OnPlayerDisconnected;

        /// <summary>
        /// ServerInstance outputs messages which are announced in this event
        /// </summary>
        public event Action<ServerInstanceOutputMessage> OnServerInstanceOutput;

        /// <summary>
        /// All players that are/were connected to the server
        /// </summary>
        public List<ServerPlayer> AllPlayers { get; } = new List<ServerPlayer>();

        /// <summary>
        /// Thread that listens to new messages from server instance
        /// </summary>
        private Thread _serverInstanceOutputThread;

        /// <summary>
        /// Creates a server instance for MineCraft server based on where the directory of MineCraft server is located
        /// </summary>
        /// <param name="fullPath">Path where server is located</param>
        internal Server(string fullPath)
        {
            FullPath = fullPath;

            ServerInstance = GetServerProcess();

            Name = FullPath.Split(Path.DirectorySeparatorChar)[^1];

            ServerProperties = new Properties(GetFilePath("server.properties"));
        }

        /// <summary>
        /// Returns a shell command that starts the minecraft server
        /// </summary>
        /// <returns></returns>
        private Process GetServerProcess()
        {
            return Utilities.RunShellCommand(
                windows: $"cd {FullPath} && bedrock_server.exe", 
                ubuntu: $"cd {FullPath} && chmod +x bedrock_server && ./bedrock_server");
        }

        /// <summary>
        /// Returns file path to file in server directory
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFilePath(string fileName)
        {
            var path = Path.Combine(FullPath, fileName);

            return File.Exists(path) ? path : null;
        }        

        /// <summary>
        /// Starts a server if it's not running
        /// </summary>
        public void StartServer()
        {
            if (Running) return;
            
            ServerInstance.Start();
            Running = true;

            _serverInstanceOutputThread = new Thread(async () =>
            {
                while (!ServerInstance.StandardOutput.EndOfStream && Running)
                {
                    var outputMessage = await ServerInstance.StandardOutput.ReadLineAsync();

                    var processedMessage = await ServerInstanceOutputMessage.Create(this, outputMessage);

                    OnServerInstanceOutput?.Invoke(processedMessage);
                }
            });

            _serverInstanceOutputThread.Start();

            ServerStartedAt = DateTime.Now;            
        }

        /// <summary>
        /// Stops a server if it's running
        /// </summary>
        public async Task StopServerAsync()
        {
            if (!Running) return;

            var time = DateTime.Now;

            foreach (var player in AllPlayers)
            {
                CallPlayerDisconnected(player, time);
            }

            await RunCommandAsync("stop");
            ServerInstance.WaitForExit();

            Running = false;
        }

        /// <summary>
        /// If server is running, calls StopServer then StartServer
        /// </summary>
        public async Task RestartServerAsync()
        {
            if (Running)
            {
                await StopServerAsync();
                StartServer();
            }
        }

        /// <summary>
        /// All worlds in "worlds" directory in this server
        /// </summary>
        /// <returns></returns>
        public string[] AvailableWorlds()
        {
            var worldsDirectory = Path.Combine(FullPath, "worlds");

            return Directory.Exists(worldsDirectory) ? Directory.GetDirectories(worldsDirectory) : null;
        }

        /// <summary>
        /// Gets permissions of players saved in permissions.json file
        /// </summary>
        public async Task<IReadOnlyCollection<Permissions>> GetPermissionsAsync()
        {
            var fileContent = await File.ReadAllTextAsync(GetFilePath("permissions.json"));
            var json = JsonConvert.DeserializeObject<IReadOnlyCollection<Permissions>>(fileContent);

            return json;
        }

        /// <summary>
        /// Runs a command on the running server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns back the command or null if command didn't run</returns>
        public async Task<Command> RunCommandAsync(Command command)
        {
            if (Running)
            {
                await ServerInstance.StandardInput.WriteLineAsync(command.MinecraftCommand);

                return command;
            }
            else
            {
                throw new Exception($"Can't run command \"{command}\" because server isn't running.");
            }
        }

        /// <summary>
        /// Runs a command on the running server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns back the command or null if command didn't run</returns>
        public async Task<Command> RunCommandAsync(string command) =>
            await RunCommandAsync(new Command(command));

        /// <summary>
        /// Creates a new ServerPlayer and calls an event that they connected
        /// </summary>
        /// <param name="username"></param>
        /// <param name="xuid"></param>
        /// <param name="when"></param>
        private void CallPlayerConnected(string username, long xuid, DateTime when)
        {
            var player = new ServerPlayer
            {
                Username = username,
                Xuid = xuid,
                LastAction = when,
                IsOnline = true,
                ServerId = ID
            };

            AllPlayers.Add(player);

            OnPlayerConnected?.Invoke(player);
        }

        /// <summary>
        /// Announces that a ServerPlayer has joined the serevr
        /// </summary>
        /// <param name="player"></param>
        /// <param name="when"></param>
        private void CallPlayerConnected(ServerPlayer player, DateTime when)
        {
            player.IsOnline = true;
            player.LastAction = when;

            OnPlayerConnected?.Invoke(player);
        }

        /// <summary>
        /// Announces that a ServerPlayer has disconnected from the server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="when"></param>
        private void CallPlayerDisconnected(ServerPlayer player, DateTime when)
        {
            player.IsOnline = false;
            player.LastAction = when;

            OnPlayerDisconnected?.Invoke(player);
        }
    }
}
