using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace MinecraftBedrockServerConfigurator
{
    class Server
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
        /// Server number this server has (located at the end of a folder)
        /// </summary>
        public int Number => int.Parse(Name.Split("_")[^1]);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverInstance">Process that links to bedrock_server file</param>
        /// <param name="name">Name of server directory</param>
        /// <param name="fullPath">Path to server directory</param>
        /// <param name="serverProperties">Properties loaded from server.properties file</param>
        public Server(Process serverInstance, string name, string fullPath, Dictionary<string, string> serverProperties)
        {
            ServerInstance = serverInstance;
            Name = name;
            FullPath = fullPath;
            ServerProperties = serverProperties;
        }

        /// <summary>
        /// Overwrites server.properties with current version of ServerProperties
        /// If server is running it's recommended to call RestartServer
        /// Call this everytime ServerProperties are updated so it will be saved
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

                Console.WriteLine("Started " + Name);
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

                Console.WriteLine("Stopped " + Name);
            }
            else
            {
                Console.WriteLine($"Stopping {Name} didn't happen because it wasn't running. " +
                                   "(You will see this message when you are exiting the program.)");
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

            Console.WriteLine($"{Name} - {message}");
        }

        /// <summary>
        /// Calls StopServer then StartServer
        /// </summary>
        public void RestartServer()
        {
            StopServer();
            StartServer();
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
                Console.WriteLine($"Can't run command \"{command}\" because server \"{Name}\" isn't running.");
            }
        }

        /// <summary>
        /// Number - Name - ServerProperties["server-name"] - ServerProperties["server-port"]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Number} - {Name} - {ServerProperties["server-name"]} - {ServerProperties["server-port"]}";
        }
    }
}
