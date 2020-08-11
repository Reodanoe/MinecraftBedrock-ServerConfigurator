using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.ServerFiles;
using Newtonsoft.Json;
using System.Threading;

namespace BedrockServerConfigurator.Library
{
    public class Server
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

        /// <summary>
        /// Logs all messages from Server
        /// </summary>
        public event Action<string> Log;

        public event Action<ServerPlayer> OnPlayerConnected;
        public event Action<ServerPlayer> OnPlayerDisconnected;

        public event Action<ServerOutputMessage> OnServerOutput;

        /// <summary>
        /// All players that are/were connected to the server
        /// </summary>
        public List<ServerPlayer> AllPlayers { get; } = new List<ServerPlayer>();

        private Thread _serverInstanceOutputThread;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverInstance">Process that links to bedrock_server file</param>
        /// <param name="name">Name of server directory</param>
        /// <param name="fullPath">Path to server directory</param>
        /// <param name="serverProperties">Properties loaded from server.properties file</param>
        internal Server(Process serverInstance, string name, string fullPath, Properties serverProperties)
        {
            ServerInstance = serverInstance;
            Name = name;
            FullPath = fullPath;
            ServerProperties = serverProperties;
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
                        await NewMessageFromServerAsync(await ServerInstance.StandardOutput.ReadLineAsync());
                    }
                });

                _serverInstanceOutputThread.Start();

                CallLog("Server started");
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
                    player.IsOnline = false;
                    player.LastAction = time;
                }

                await RunCommandAsync("stop");
                ServerInstance.WaitForExit();

                Running = false;
                
                CallLog("Server stopped");
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
        /// When ServerInstance writes new line of message this method gets called which works with it
        /// </summary>
        /// <param name="message"></param>
        private async Task NewMessageFromServerAsync(string message)
        {
            CallLog(message);

            var msg = await ServerOutputMessage.Create(this, message);

            OnServerOutput?.Invoke(msg);
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
                CallLog($"Can't run command \"{command}\" because server isn't running.");

                return null;
            }
        }

        /// <summary>
        /// Runs a command on the running server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns back the command or null if command didn't run</returns>
        public async Task<Command> RunCommandAsync(string command) =>
            await RunCommandAsync(new Command(command));

        internal void CallPlayerConnected(ServerPlayer player)
        {
            OnPlayerConnected?.Invoke(player);
        }

        internal void CallPlayerDisconnected(ServerPlayer player)
        {
            OnPlayerDisconnected?.Invoke(player);
        }

        private void CallLog(string message)
        {
            Log?.Invoke(message);
        }
    }
}
