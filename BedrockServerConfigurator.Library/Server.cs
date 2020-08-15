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
        public bool Running { get; private set; } = false;

        /// <summary>
        /// ID of a server (number at the end of the name of folder where server is located)
        /// </summary>
        public int ID => int.Parse(Name.Split("_")[^1]);

        /// <summary>
        /// Version of Minecraft server
        /// </summary>
        public string Version => File.ReadAllLines(GetFilePath("version.txt"))[0];

        public event Action<ServerPlayer> OnPlayerConnected;
        public event Action<ServerPlayer> OnPlayerDisconnected;

        public event Action<ServerInstanceOutputMessage> OnServerInstanceOutput;

        /// <summary>
        /// All players that are/were connected to the server
        /// </summary>
        public List<ServerPlayer> AllPlayers { get; } = new List<ServerPlayer>();

        private Thread _serverInstanceOutputThread;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath">Path where server is located</param>
        internal Server(string fullPath)
        {
            FullPath = fullPath;

            ServerInstance = GetServerProcess();

            Name = FullPath.Split(Path.DirectorySeparatorChar)[^1];

            ServerProperties = new Properties(GetFilePath("server.properties"));
        }

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
            if (!Running)
            {
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
            }
        }

        /// <summary>
        /// Stops a server if it's running
        /// </summary>
        public async Task StopServerAsync()
        {
            if (Running)
            {
                var time = DateTime.Now;

                foreach (var player in AllPlayers)
                {
                    CallPlayerDisconnected(player, time);
                }

                await RunCommandAsync("stop");
                ServerInstance.WaitForExit();

                Running = false;
            }
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
