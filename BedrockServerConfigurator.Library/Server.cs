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

        public Permissions[] Permissions => JsonConvert.DeserializeObject<Permissions[]>(File.ReadAllText(GetFilePath("permissions.json")));

        /// <summary>
        /// Logs all messages from Server
        /// </summary>
        public event Action<string> Log;

        public event Action<ServerPlayer> PlayerConnected;
        public event Action<ServerPlayer> PlayerDisconnected;

        /// <summary>
        /// All players that are/were connected to the server
        /// </summary>
        public List<ServerPlayer> AllPlayers { get; } = new List<ServerPlayer>();

        private Task _messagesTask;

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

                _messagesTask = Task.Run(async () => {
                    while (!ServerInstance.StandardOutput.EndOfStream && Running)
                    {
                        NewMessageFromServer(await ServerInstance.StandardOutput.ReadLineAsync());
                    }
                });

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
                await RunCommandAsync("stop");
                Running = false;
                ServerInstance.WaitForExit();

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
        private void NewMessageFromServer(string message)
        {
            CallLog(message);

            if(message.Contains("Player") && message.Contains("connected"))
            {
                SetPlayerOnlineOrOffline(message);
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

        // this should maybe be like in a server message processor class
        private void SetPlayerOnlineOrOffline(string message)
        {
            // [2020-07-19 18:29:49 INFO] Player connected: PLAYER_NAME, xuid: ID
            // [2020-07-19 18:30:57 INFO] Player disconnected: PLAYER_NAME, xuid: ID

            var split = message.Split(':');

            var date = Utilities.GetDateTimeFromServerMessage(message);
            var username = split[^2].Split(',')[0].Trim();  // " PLAYER_NAME, xuid: ID" -> " PLAYER_NAME" -> "PLAYER_NAME"
            var xuid = long.Parse(split[^1].Trim());        // " ID" -> (long)"ID"

            var player = AllPlayers.FirstOrDefault(x => x.Xuid == xuid);

            // but what if there's a player called disconnected and they connected ...
            if (message.Contains("disconnected"))
            {
                // if server glitched and player never actually connected
                if (player == null) return;

                player.IsOnline = false;
                player.LastAction = date;

                PlayerDisconnected?.Invoke(player);
            }
            else
            {
                if (player == null)
                {
                    player = new ServerPlayer
                    {
                        Username = username,
                        Xuid = xuid,
                        IsOnline = true,
                        LastAction = date,
                        ServerId = ID
                    };

                    AllPlayers.Add(player);
                }
                else
                {
                    player.IsOnline = true;
                    player.LastAction = date;
                }

                PlayerConnected?.Invoke(player);
            }
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

        private void CallLog(string message)
        {
            Log?.Invoke(message);
        }
    }
}
