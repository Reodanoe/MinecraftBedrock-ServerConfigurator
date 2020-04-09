using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace BedrockServerConfigurator
{
    public class Server
    {
        public Process ServerInstance { get; }
        public string Name { get; }
        public string FullPath { get; }
        public Dictionary<string, string> ServerProperties { get; }

        public bool Running { get; private set; } = false;

        /// <summary>
        /// Converts ServerProperties into a readable string
        /// </summary>
        public string Properties => string.Join(Environment.NewLine, ServerProperties.Select(x => $"{x.Key}={x.Value}"));

        /// <summary>
        /// ID of a server (number at the end of the name of folder where server is located)
        /// </summary>
        public int ID => int.Parse(Name.Split("_")[^1]);

        /// <summary>
        /// Logs all messages from Server
        /// </summary>
        public event EventHandler<string> Log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverInstance">Process that links to bedrock_server file</param>
        /// <param name="name">Name of server directory</param>
        /// <param name="fullPath">Path to server directory</param>
        /// <param name="serverProperties">Properties loaded from server.properties file</param>
        internal Server(Process serverInstance, string name, string fullPath, Dictionary<string, string> serverProperties)
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
            File.WriteAllText(Path.Combine(FullPath, "server.properties"), Properties);
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

                Task.Run(() => {
                    while (!ServerInstance.StandardOutput.EndOfStream && Running)
                    {
                        NewMessageFromServer(ServerInstance.StandardOutput.ReadLine());
                    }
                });

                CallLog("Started " + Name);
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

                CallLog("Stopped " + Name);
            }
            else
            {
                CallLog($"Stopping {Name} didn't happen because it wasn't running. " +
                                   "(You will see this message when you are exiting the program.)");
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
            // I could implement here more features
            //      keeping track of joined players in this class
            // [2020-03-13 11:57:28 INFO] Player connected: playerName, xuid: number

            CallLog($"{Name} - {message}");
        }

        /// <summary>
        /// Runs a command on the running server.
        /// </summary>
        /// <param name="command"></param>
        public void RunACommand(string command)
        {
            if (Running)
            {
                ServerInstance.StandardInput.WriteLine(command);
            }
            else
            {
                CallLog($"Can't run command \"{command}\" because server \"{Name}\" isn't running.");
            }
        }

        /// <summary>
        /// ID - Name - ["server-name"] - ["server-port"]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ID} - {Name} - {ServerProperties["server-name"]} - {ServerProperties["server-port"]}";
        }

        private void CallLog(string message)
        {
            Log?.Invoke(null, message);
        }
    }
}
