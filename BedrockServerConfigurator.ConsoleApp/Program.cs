using System.Threading.Tasks;
using BedrockServerConfigurator.Library;

namespace BedrockServerConfigurator.ConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            await EasyStart();
        }

        private static async Task EasyStart()
        {
            // creates a directory "servers" in Directory.GetCurrentDirectory()
            // each server will be named bedrockServer and have an ID append to it
            var config = new Configurator("servers", "bedrockServer");

            // downloads "template" server
            // template server is used for creating other servers by copying it to a new directory and giving it an ID
            /// TIP: Try handling event `config.TemplateServerDownloadChanged` to see download progress in action
            await config.DownloadBedrockServer();

            // creates a new server in servers directory
            // this is then used as an actual server that runs
            config.CreateNewServer();

            // let's make another one
            config.CreateNewServer();

            /// NOTE: Once you created servers you're going to use and you're going to launch your program again
            ///       don't forget to not create any new servers anymore so your server folder doesn't become full
            ///       and your ram won't be taken up by instances of servers you aren't using
            ///       to see what servers you have created so far use `config.AllServerDirectories()`

            // this must be called so servers get instantiated
            // this method also assigns each server their unique port so there are no conflicts
            config.LoadServers();

            // this now starts both created servers
            // try connecting at port 19134 for server with ID 1 or 19136 for the second server with ID 2
            config.StartAllServers();

            // you need to handle stopping servers for example by using `config.StopAllServers()`
            // so they won't still be running once you turn off your program

            /// TIPS:
            /// * Check out servers using the property `config.AllServersList` or `config.AllServers` to access valuable information
            /// * Note that you cannot rename servers just by changing name in constructor of Configurator instance on next launch, once you go with one keep with it
            /// * To run a command on a server you can use `config.RunCommandOnSpecifiedServer(int serverId, string command)` 
            ///   or by accessing a server directly through one of mentioned collections and using `RunACommand(string command)`
            ///   But much easier way is to use GetServerApi(int serverId) to instantiate ServerApi and use its methods which are prepared for you to use
            /// * Listen to event `config.Log` and `OnServerInstanceOutput` in Server class for valuable information

            // Stop this program from quitting instantly
            await Task.Delay(-1);
        }
    }
}
