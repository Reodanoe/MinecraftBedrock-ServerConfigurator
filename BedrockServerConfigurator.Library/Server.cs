using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace BedrockServerConfigurator.Library
{
    public class Server
    {
        public Process ServerInstance { get; }
        public string Name { get; }
        public string FullPath { get; }
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
        public event EventHandler<string> Log;

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

                Task.Run(() => {
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
            Log?.Invoke(null, message);
        }
    }
}
