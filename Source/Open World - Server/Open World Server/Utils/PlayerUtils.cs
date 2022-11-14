﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenWorldServer
{
    public class PlayerUtils
    {
        private readonly SaveSystem _saveSystem;
        private readonly WorldUtils _worldUtils;
        public PlayerUtils(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
        }

        

        public void SaveNewPlayerFile(string username, string password)
        {
            ServerClient playerToOverwrite = Server.savedClients.Find(fetch => fetch.username == username);

            if (playerToOverwrite != null)
            {
                if (!string.IsNullOrWhiteSpace(playerToOverwrite.homeTileID))
                    _worldUtils.RemoveSettlement(playerToOverwrite, playerToOverwrite.homeTileID);
                playerToOverwrite.wealth = 0;
                playerToOverwrite.pawnCount = 0;
                _saveSystem.SavePlayer(playerToOverwrite);
                return;
            }

            ServerClient dummy = new ServerClient(null);
            dummy.username = username;
            dummy.password = password;

            Server.savedClients.Add(dummy);
            _saveSystem.SavePlayer(dummy);
        }

        public void GiveSavedDataToPlayer(ServerClient client)
        {
            ServerClient savedClient = Server.savedClients.Find(fetch => fetch.username == client.username);

            if (savedClient == null) return;

            client.username = savedClient.username;
            client.homeTileID = savedClient.homeTileID;
            client.giftString = savedClient.giftString.ToList();
            client.tradeString = savedClient.tradeString.ToList();

            savedClient.giftString.Clear();
            savedClient.tradeString.Clear();

            if (savedClient.faction != null)
            {
                Faction factionToGive = Server.savedFactions.Find(fetch => fetch.name == savedClient.faction.name);
                if (factionToGive != null) client.faction = factionToGive;
                else client.faction = null;
            }

            _saveSystem.SavePlayer(savedClient);
        }

        public void CheckAllAvailablePlayers(bool newLine)
        {
            if (newLine) Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Players Check:");
            Console.ForegroundColor = ConsoleColor.White;

            CheckSavedPlayers();
            CheckForBannedPlayers();
            CheckForWhitelistedPlayers();
            Console.WriteLine("");
        }

        private void CheckSavedPlayers()
        {
            Server.savedClients.Clear();
            Server.savedSettlements.Clear();

            if (!Directory.Exists(Server.playersFolderPath))
            {
                Directory.CreateDirectory(Server.playersFolderPath);
                ConsoleUtils.LogToConsole("No Players Folder Found, Generating");
                return;
            }

            else
            {
                string[] playerFiles = Directory.GetFiles(Server.playersFolderPath);

                foreach (string file in playerFiles)
                {
                    if (Server.usingIdleTimer)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.LastAccessTime < DateTime.Now.AddDays(-Server.idleTimer))
                        {
                            fi.Delete();
                        }
                    }

                    _saveSystem.LoadPlayer(file);
                }

                if (Server.savedClients.Count == 0) ConsoleUtils.LogToConsole("No Saved Players Found, Ignoring");
                else ConsoleUtils.LogToConsole("Loaded [" + Server.savedClients.Count + "] Player Files");
            }
        }

        private void CheckForBannedPlayers()
        {
            Server.bannedIPs.Clear();

            if (!File.Exists(Server.mainFolderPath + Path.DirectorySeparatorChar + "bans_ip.dat"))
            {
                ConsoleUtils.LogToConsole("No Bans File Found, Ignoring");
                return;
            }

            Server.bannedIPs = _saveSystem.LoadBans();

            if (Server.bannedIPs.Count == 0) ConsoleUtils.LogToConsole("No Banned Players Found, Ignoring");
            else ConsoleUtils.LogToConsole("Loaded [" + Server.bannedIPs.Count + "] Banned Players");
        }

        private static void CheckForWhitelistedPlayers()
        {
            Server.whitelistedUsernames.Clear();

            if (!File.Exists(Server.whitelistedUsersPath))
            {
                File.Create(Server.whitelistedUsersPath);

                ConsoleUtils.LogToConsole("No Whitelisted Players File Found, Generating");
            }

            else
            {
                if (File.ReadAllLines(Server.whitelistedUsersPath).Count() == 0) ConsoleUtils.LogToConsole("No Whitelisted Players Found, Ignoring");
                else
                {
                    foreach (string str in File.ReadAllLines(Server.whitelistedUsersPath))
                    {
                        Server.whitelistedUsernames.Add(str);
                    }

                    ConsoleUtils.LogToConsole("Loaded [" + Server.whitelistedUsernames.Count + "] Whitelisted Players");
                }
            }
        }

        public void CheckForPlayerWealth(ServerClient client)
        {
            if (Server.usingWealthSystem == false) return;
            if (Server.banWealthThreshold == 0 && Server.warningWealthThreshold == 0) return;
            if (client.isAdmin) return;

            int wealthToCompare = (int) Server.savedClients.Find(fetch => fetch.username == client.username).wealth;

            if (client.wealth - wealthToCompare > Server.banWealthThreshold && Server.banWealthThreshold > 0)
            {
                _saveSystem.SavePlayer(client);
                Server.savedClients.Find(fetch => fetch.username == client.username).wealth = client.wealth;
                Server.savedClients.Find(fetch => fetch.username == client.username).pawnCount = client.pawnCount;

                Server.bannedIPs.Add(((IPEndPoint)client.tcp.Client.RemoteEndPoint).Address.ToString(), client.username);
                client.disconnectFlag = true;
                _saveSystem.SaveBans(Server.bannedIPs);

                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.LogToConsole("Player [" + client.username + "]'s Wealth Triggered Alarm [" + wealthToCompare + " > " + (int)client.wealth + "], Banning");
                Console.ForegroundColor = ConsoleColor.White;
            }

            else if (client.wealth - wealthToCompare > Server.warningWealthThreshold && Server.warningWealthThreshold > 0)
            {
                _saveSystem.SavePlayer(client);
                Server.savedClients.Find(fetch => fetch.username == client.username).wealth = client.wealth;
                Server.savedClients.Find(fetch => fetch.username == client.username).pawnCount = client.pawnCount;

                Console.ForegroundColor = ConsoleColor.Yellow;
                ConsoleUtils.LogToConsole("Player [" + client.username + "]'s Wealth Triggered Warning [" + wealthToCompare + " > " + (int) client.wealth + "]");
                Console.ForegroundColor = ConsoleColor.White;
            }

            else
            {
                _saveSystem.SavePlayer(client);
                Server.savedClients.Find(fetch => fetch.username == client.username).wealth = client.wealth;
                Server.savedClients.Find(fetch => fetch.username == client.username).pawnCount = client.pawnCount;
            }
        }

        public static bool CheckForConnectedPlayers(string tileID)
        {
            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient client in clients)
            {
                if (client.homeTileID == tileID) return true;
            }

            return false;
        }

        public static ServerClient GetPlayerFromTile(string tileID)
        {
            return Networking.connectedClients.Find(fetch => fetch.homeTileID == tileID);
        }

        public static bool CheckForPlayerShield(string tileID)
        {
            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient client in clients)
            {
                if (client.homeTileID == tileID && !client.eventShielded && !client.isImmunized)
                {
                    client.eventShielded = true;
                    return true;
                }
            }

            return false;
        }

        public static string GetSpyData(string tileID, ServerClient origin)
        {
            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient client in clients)
            {
                if (client.homeTileID == tileID)
                {
                    string dataToReturn = client.pawnCount.ToString() + "»" + client.wealth.ToString() + "»" + client.eventShielded + "»" + client.inRTSE;

                    if (client.giftString.Count > 0) dataToReturn += "»" + "True";
                    else dataToReturn += "»" + "False";

                    if (client.tradeString.Count > 0) dataToReturn += "»" + "True";
                    else dataToReturn += "»" + "False";

                    Random rnd = new Random();
                    int chance = rnd.Next(0, 2);
                    if (chance == 1) Networking.SendData(client, "Spy│" + origin.username);

                    ConsoleUtils.LogToConsole("Spy Done Between [" + origin.username + "] And [" + client.username + "]");

                    return dataToReturn;
                }
            }

            return "";
        }

        public static void SendEventToPlayer(ServerClient invoker, string data)
        {
            string dataToSend = "ForcedEvent│" + data.Split('│')[1];

            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in clients)
            {
                if (sc.homeTileID == data.Split('│')[2])
                {
                    ConsoleUtils.LogToConsole("Player [" + invoker.username + "] Has Sent Forced Event [" + data.Split('│')[1] + "] To [" + sc.username + "]");
                    Networking.SendData(sc, dataToSend);
                    break;
                }
            }
        }

        public void SendGiftToPlayer(ServerClient invoker, string data)
        {
            string tileToSend = data.Split('│')[1];
            string dataToSend = "GiftedItems│" + data.Split('│')[2];

            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in clients)
            {
                if (sc.homeTileID == tileToSend)
                {
                    Networking.SendData(sc, dataToSend);
                    ConsoleUtils.LogToConsole("Gift Done Between [" + invoker.username + "] And [" + sc.username + "]");
                    return;
                }
            }

            dataToSend = dataToSend.Replace("GiftedItems│", "");

            ServerClient[] savedClients = Server.savedClients.ToArray();
            foreach(ServerClient sc in savedClients)
            {
                if (sc.homeTileID == tileToSend)
                {
                    sc.giftString.Add(dataToSend);
                    _saveSystem.SavePlayer(sc);
                    ConsoleUtils.LogToConsole("Gift Done Between [" + invoker.username + "] And [" + sc.username + "] But Was Offline. Saving");
                    return;
                }
            }
        }

        public static void SendTradeRequestToPlayer(ServerClient invoker, string data)
        {
            string dataToSend = "TradeRequest│" + invoker.username + "│" + data.Split('│')[2] + "│" + data.Split('│')[3];

            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in clients)
            {
                if (sc.homeTileID == data.Split('│')[1])
                {
                    Networking.SendData(sc, dataToSend);
                    return;
                }
            }
        }

        public static void SendBarterRequestToPlayer(ServerClient invoker, string data)
        {
            string dataToSend = "BarterRequest│" + invoker.homeTileID + "│" + data.Split('│')[2];

            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in clients)
            {
                if (sc.homeTileID == data.Split('│')[1])
                {
                    Networking.SendData(sc, dataToSend);
                    return;
                }
            }
        }
    }
}
