using System;
using System.IO;
using System.Linq;
using BedrockServerConfigurator;

namespace ServerConfigurator
{
    class Program
    {
        // TODO
        //

        private static Configurator config;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (a, b) =>
            {
                config.StopAllServers();
                Environment.Exit(0);
            };

            try
            {
                BeginConfig();
                Menu();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void BeginConfig()
        {
            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            config = new Configurator(
                Path.Combine(defaultPath, "bedrockServers"),
                "bedServer");

            config.Log += Log;
        }

        private static void Log(object sender, string message)
        {
            Console.WriteLine(message);
        }

        private static void Menu()
        {
            // Download server
            while (true)
            {
                Console.Write("Dowload new server? [Y/N]: ");
                var newServer = Console.ReadLine().ToUpper();

                if (newServer == "Y")
                {
                    config.DownloadBedrockServer();

                    break;
                }
                else if (newServer == "N")
                {
                    break;
                }
            }

            // Create new servers
            while (true)
            {
                // there already has to be a template server for this to work
                Console.Write("Create new servers? [Y/N]: ");
                var servers = Console.ReadLine().ToUpper();

                if (servers == "Y")
                {
                    Console.Write("How many?: ");
                    var amount = Convert.ToInt32(Console.ReadLine());

                    for (int i = 0; i < amount; i++)
                    {
                        config.NewServer();
                    }

                    break;
                }
                else if (servers == "N")
                {
                    break;
                }
            }

            config.LoadServers();

            // Start servers
            while (true)
            {
                Console.Write("Start all loaded servers? [Y/N]: ");
                var start = Console.ReadLine().ToUpper();

                if (start == "Y")
                {
                    config.StartAllServers();
                    break;
                }
                else if (start == "N")
                {
                    break;
                }
            }

            // Quit or run commands
            while (true)
            {
                string quitKeyword = "quit";

                Console.WriteLine($"To quit the program type \"{quitKeyword}\". Or run a command on a server using \"[server ID] - [command]\".");

                var input = Console.ReadLine();

                if (input == quitKeyword)
                {
                    config.StopAllServers();
                    break;
                }
                else
                {
                    switch (input)
                    {
                        case "restart":
                            config.RestartAllServers();
                            break;
                        default:
                            var serverCommand = input.Split("-");

                            if (serverCommand.Length == 2)
                            {
                                var server = serverCommand[0].Trim();

                                if (int.TryParse(server, out int serverNumber))
                                {
                                    config.RunCommandOnSpecifiedServer(serverNumber, serverCommand[1].Trim());
                                }
                                else
                                {
                                    Console.WriteLine($"{server} is not a number");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid input.");
                            }
                            break;
                    }
                }
            }
        }
    }
}
