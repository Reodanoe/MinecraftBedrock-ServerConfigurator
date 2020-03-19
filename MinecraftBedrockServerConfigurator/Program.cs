using System;
using System.IO;
using System.Linq;

namespace MinecraftBedrockServerConfigurator
{
    class Program
    {
        //
        // TODO
        //
        // choose on which server you want the command to run
        // use args in main method to configure the app
        // implement backups in configurator
        //

        private static Configurator config;

        static void Main(string[] args)
        {
            BeginConfig();
            Menu();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void BeginConfig()
        {
            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
            config = new Configurator(
                Path.Combine(defaultPath, "bedrockServers"),
                "bedServer");
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

                Console.WriteLine($"To quit the program type \"{quitKeyword}\". Or for now type a command.");

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
                            config.AllServersAction(y => y.RunACommand(input));
                            break;
                    }
                }
            }
        }
    }
}
