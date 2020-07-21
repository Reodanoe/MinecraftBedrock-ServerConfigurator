using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Commands;

namespace BedrockServerConfigurator.Library
{
    public class Server
    {
        public Process ServerInstance { get; }
        public string Name { get; }

        /// <summary>
        /// The path where server is installed
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Manipulates with server.properties file
        /// </summary>
        public Properties ServerProperties { get; }

        public bool Running { get; private set; } = false;        

        /// <summary>
        /// ID of a server (number at the end of the name of folder where server is located)
        /// </summary>
        public int ID => int.Parse(Name.Split("_")[^1]);

        /// <summary>
        /// Gets version of minecraft server
        /// </summary>
        public string Version => File.ReadAllLines(Path.Combine(FullPath, "version.txt"))[0];

        /// <summary>
        /// Logs all messages from Server
        /// </summary>
        public event Action<string> Log;

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
        /// Overwrites server.properties with current version of ServerProperties.
        /// If server is running it's recommended to call RestartServer.
        /// Call this everytime ServerProperties are updated so they will be saved.
        /// </summary>
        public void UpdateProperties()
        {
            File.WriteAllText(Path.Combine(FullPath, "server.properties"), ServerProperties.ToString());
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

                _messagesTask = Task.Run(() => {
                    while (!ServerInstance.StandardOutput.EndOfStream && Running)
                    {
                        NewMessageFromServer(ServerInstance.StandardOutput.ReadLine());
                    }
                });

                CallLog("Server started");
            }
        }

        /// <summary>
        /// Stops a server if it's running
        /// </summary>
        public void StopServer()
        {
            if (Running)
            {
                RunACommand("stop");
                Running = false;
                ServerInstance.WaitForExit();

                CallLog("Server stopped");
            }
        }

        /// <summary>
        /// If server is running, calls StopServer then StartServer
        /// </summary>
        public void RestartServer()
        {
            if (Running)
            {
                StopServer();
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
                PlayerAction(message);
            }
        }

        /// <summary>
        /// All worlds in "worlds" directory in this server
        /// </summary>
        /// <returns></returns>
        public string[] AvailableWorlds()
        {
            var worldsDirectory = Path.Combine(FullPath, "worlds");

            if (Directory.Exists(worldsDirectory)) 
            {
                var result = Directory.GetDirectories(worldsDirectory);

                return result;
            }

            return null;
        }

        // this should maybe be like in a server message processor class
        private void PlayerAction(string message)
        {
            // [2020-07-19 18:29:49 INFO] Player connected: PLAYER_NAME, xuid: ID
            // [2020-07-19 18:30:57 INFO] Player disconnected: PLAYER_NAME, xuid: ID

            var split = message.Split(':');

            var date = Utilities.GetDateTimeFromServerMessage(message);
            var username = split[^2].Split(',')[0].Trim();
            var xuid = long.Parse(split[^1].Trim());

            var joinedPlayer = AllPlayers.FirstOrDefault(x => x.Xuid == xuid);

            if (message.Contains("disconnected"))
            {
                joinedPlayer.IsOnline = false;
                joinedPlayer.LastAction = date;
            }
            else
            {
                if (joinedPlayer == null)
                {
                    AllPlayers.Add(new ServerPlayer
                    {
                        Username = username,
                        Xuid = xuid,
                        IsOnline = true,
                        LastAction = date,
                        ServerId = ID
                    });
                }
                else
                {
                    joinedPlayer.IsOnline = true;
                    joinedPlayer.LastAction = date;
                }
            }
        }

        /// <summary>
        /// Runs a command on the running server.
        /// </summary>
        /// <param name="command"></param>
        public void RunACommand(Command command) => RunACommand(command.ToString());

        /// <summary>
        /// Runs a command on the running server.
        /// </summary>
        /// <param name="command"></param>
        public void RunACommand(string command)
        {
            Console.WriteLine(command);

            if (Running)
            {
                ServerInstance.StandardInput.WriteLine(command);
            }
            else
            {
                CallLog($"Can't run command \"{command}\" because ServerInstance isn't running.");
            }
        }

        /// <summary>
        /// ID - Name - ["server-name"] - ["server-port"]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ID} - {Name} - {ServerProperties.ServerName} - {ServerProperties.ServerPort}";
        }

        private void CallLog(string message)
        {
            Log?.Invoke(message);
        }
    }
}
