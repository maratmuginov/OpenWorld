using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenWorldServer
{
    public class Networking
    {
        private readonly NetworkingHandler _networkingHandler;
        private readonly FactionBankHandler _factionBankHandler;
        private readonly Dictionary<string, Action<ServerClient, string>> clientMessageHandlers;

        public Networking(NetworkingHandler networkingHandler, FactionBankHandler factionBankHandler)
        {
            _networkingHandler = networkingHandler;
            clientMessageHandlers = new Dictionary<string, Action<ServerClient, string>>()
            {
                {"Connect", _networkingHandler.ConnectHandle },
                {"ChatMessage", _networkingHandler.ChatMessageHandle },
                {"UserSettlement", _networkingHandler.UserSettlementHandle },
                {"ForceEvent", _networkingHandler.ForceEventHandle },
                {"SendGiftTo", _networkingHandler.SendGiftHandle },
                {"SendTradeTo", _networkingHandler.SendTradeHandle },
                {"SendBarterTo", _networkingHandler.SendBarterHandle },
                {"TradeStatus", _networkingHandler.TradeStatusHandle },
                {"BarterStatus", _networkingHandler.BarterStatusHandle },
                {"GetSpyInfo", _networkingHandler.SpyInfoHandle },
                {"FactionManagement", _networkingHandler.FactionManagementHandle }
            };
            _factionBankHandler = factionBankHandler;
        }

        private static TcpListener server;
        public static IPAddress localAddress;
        public static int serverPort = 0;
        public static List<ServerClient> connectedClients = new();

        public void ReadyServer()
        {
            server = new TcpListener(localAddress, serverPort);
            server.Start();

            ConsoleUtils.UpdateTitle();

            Thread checkThread = new(CheckClientsConnection)
            {
                IsBackground = true,
                Name = "Check Thread"
            };
            checkThread.Start();

            Thread factionProductionSiteTickThread = new(FactionProductionSiteHandler.TickProduction)
            {
                IsBackground = true,
                Name = "Factions Production Site Tick Thread"
            };
            factionProductionSiteTickThread.Start();

            Thread factionBankTickThread = new(_factionBankHandler.TickBank)
            {
                IsBackground = true,
                Name = "Factions Bank Tick Thread"
            };
            factionBankTickThread.Start();

            while (true)
                ListenForIncomingUsers();
        }

        private void ListenForIncomingUsers()
        {
            ServerClient newServerClient = new(server.AcceptTcpClient());

            connectedClients.Add(newServerClient);

            var clientThread = new Thread(() => ListenToClient(newServerClient));
            clientThread.IsBackground = true;
            clientThread.Name = "User Thread " + newServerClient.username;
            clientThread.Start();
        }

        public void ListenToClient(ServerClient client)
        {
            NetworkStream stream = client.tcp.GetStream();
            StreamReader streamReader = new(stream, true);

            while (true)
            {
                try
                {
                    if (client.disconnectFlag)
                        return;

                    string encryptedData = streamReader.ReadLine();
                    string data = Encryption.DecryptString(encryptedData);

                    if (string.IsNullOrEmpty(data))
                    {
                        client.disconnectFlag = true;
                        return;
                    }

                    string networkingCommand = data[..data.IndexOf('|')];

                    if (clientMessageHandlers.TryGetValue(networkingCommand, out Action<ServerClient, string> handler))
                        handler.Invoke(client, data);
                }
                catch
                {
                    client.disconnectFlag = true;
                    return;
                }
            }
        }

        public static void SendData(ServerClient client, string data)
        {
            try
            {
                NetworkStream stream = client.tcp.GetStream();
                StreamWriter streamWriter = new(stream);

                streamWriter.WriteLine(Encryption.EncryptString(data));
                streamWriter.Flush();
            }
            catch 
            {
                client.disconnectFlag = true;
            }
        }

        public static void KickClients(ServerClient client)
        {
            connectedClients.Remove(client);

            client.tcp.Dispose();

            ConsoleUtils.LogToConsole("Player [" + client.username + "] Has Disconnected");
        }

        public static void CheckClientsConnection()
        {
            ConsoleUtils.DisplayNetworkStatus();

            while (true)
            {
                Thread.Sleep(1000);

                ServerClient[] actualClients = connectedClients.ToArray();

                var clientsToDisconnect = new List<ServerClient>();
                var clientsToPing = new List<ServerClient>();

                foreach (ServerClient client in actualClients)
                {
                    if (client.disconnectFlag)
                        clientsToDisconnect.Add(client);
                    else
                        clientsToPing.Add(client);
                }

                clientsToPing.ForEach(client => SendData(client, "Ping"));

                foreach (ServerClient client in clientsToDisconnect)
                    KickClients(client);

                if (clientsToDisconnect.Count > 0)
                {
                    ConsoleUtils.UpdateTitle();
                    ServerUtils.SendPlayerListToAll(null);
                }
            }
        }
    }
}
