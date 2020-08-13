using System;
using System.Linq;
using System.Threading.Tasks;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library
{
    public partial class Server
    {
        public class ServerInstanceOutputMessage
        {
            public Server Server { get; }
            public string Message { get; }
            public DateTime CreatedOn { get; }

            internal static async Task<ServerInstanceOutputMessage> Create(Server sender, string message)
            {
                var serverOutputMessage = new ServerInstanceOutputMessage(sender, message);
                await serverOutputMessage.ProcessMessage();

                return serverOutputMessage;
            }

            private ServerInstanceOutputMessage(Server sender, string message)
            {
                try
                {
                    CreatedOn = GetDateTimeFromServerMessage(message);
                }
                catch
                {
                    CreatedOn = DateTime.Now;
                }

                Server = sender;
                Message = message;
            }

            private async Task ProcessMessage()
            {
                if (Message.Contains("Player") && Message.Contains("connected"))
                {
                    SetPlayerOnlineOrOffline();
                }
                else if (Message.Contains("Network port occupied, can't start server."))
                {
                    await Server.StopServerAsync();

                    throw new Exception($"Port {Server.ServerProperties.ServerPort} is occupied, please close the application which is using it.");
                }
            }

            private void SetPlayerOnlineOrOffline()
            {
                // [2020-07-19 18:29:49 INFO] Player connected: PLAYER_NAME, xuid: ID
                // [2020-07-19 18:30:57 INFO] Player disconnected: PLAYER_NAME, xuid: ID

                var split = Message.Split(':');
                var username = split[^2].Split(',')[0].Trim();  // " PLAYER_NAME, xuid: ID" -> " PLAYER_NAME" -> "PLAYER_NAME"
                var xuid = long.Parse(split[^1].Trim());        // " ID" -> (long)"ID"

                var player = Server.AllPlayers.FirstOrDefault(x => x.Xuid == xuid);

                // but what if there's a player called disconnected and they connected ...
                if (Message.Contains("disconnected"))
                {
                    // if server glitched and player never actually connected
                    if (player == null) return;

                    Server.CallPlayerDisconnected(player, CreatedOn);
                }
                else
                {
                    if (player == null)
                    {
                        Server.CallPlayerConnected(username, xuid, CreatedOn);
                    }
                    else
                    {
                        Server.CallPlayerConnected(player, CreatedOn);
                    }
                }
            }

            /// <summary>
            /// Returns date from ServerInstance's output
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            public static DateTime GetDateTimeFromServerMessage(string message)
            {
                if (!message.Contains('[') || !message.Contains(']'))
                {
                    throw new FormatException("Message doesn't contain date and time");
                }

                var indexWhereDateBegins = message.IndexOf('[') + 1;
                var dateEndsAt = indexWhereDateBegins + 19;

                var datePart = message[indexWhereDateBegins..dateEndsAt];

                if (DateTime.TryParse(datePart, out DateTime result))
                {
                    return result;
                }
                else
                {
                    throw new FormatException("Cannot obtain DateTime from server message");
                }
            }
        }
    }
}
