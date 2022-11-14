﻿namespace OpenWorldServer
{
    public class NetworkingHandler
    {
        private readonly PlayerUtils _playerUtils;
        private readonly FactionHandler _factionHandler;
        private readonly WorldUtils _worldUtils;
        private readonly JoiningsUtils _joiningsUtils;
        private readonly FactionBuildingHandler _factionBuildingHandler;
        private readonly FactionSiloHandler _factionSiloHandler;
        private readonly FactionBankHandler _factionBankHandler;

        public NetworkingHandler(PlayerUtils playerUtils, FactionHandler factionHandler, JoiningsUtils joiningsUtils, FactionBuildingHandler factionBuildingHandler, WorldUtils worldUtils, FactionSiloHandler factionSiloHandler)
        {
            _playerUtils = playerUtils;
            _factionHandler = factionHandler;
            _joiningsUtils = joiningsUtils;
            _factionBuildingHandler = factionBuildingHandler;
            _worldUtils = worldUtils;
            _factionSiloHandler = factionSiloHandler;
        }

        public void ConnectHandle(ServerClient client, string data)
        {
            _joiningsUtils.LoginProcedures(client, data);
        }

        public void ChatMessageHandle(ServerClient client, string data)
        {
            ServerUtils.SendChatMessage(client, data);
        }

        public void UserSettlementHandle(ServerClient client, string data)
        {
            if (data.StartsWith("UserSettlement│NewSettlementID│"))
            {
                try
                {
                    client.wealth = float.Parse(data.Split('│')[3]);
                    client.pawnCount = int.Parse(data.Split('│')[4]);

                    _playerUtils.CheckForPlayerWealth(client);
                }
                catch { }

                _worldUtils.CheckForTileDisponibility(client, data.Split('│')[2]);
            }

            else if (data.StartsWith("UserSettlement│AbandonSettlementID│"))
            {
                if (client.homeTileID != data.Split('│')[2] || string.IsNullOrWhiteSpace(client.homeTileID)) return;
                else _worldUtils.RemoveSettlement(client, data.Split('│')[2]);
            }

            else if (data == "UserSettlement│NoSettlementInLoad")
            {
                if (string.IsNullOrWhiteSpace(client.homeTileID)) return;
                else _worldUtils.RemoveSettlement(client, client.homeTileID);
            }
        }

        public void ForceEventHandle(ServerClient client, string data)
        {
            string dataToSend;
            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[2]))
            {
                if (PlayerUtils.CheckForPlayerShield(data.Split('│')[2]))
                {
                    dataToSend = "│SentEvent│Confirm│";

                    PlayerUtils.SendEventToPlayer(client, data);
                }

                else dataToSend = "│SentEvent│Deny│";
            }
            else dataToSend = "│SentEvent│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public void SendGiftHandle(ServerClient client, string data)
        {
            _playerUtils.SendGiftToPlayer(client, data);
        }

        public void SendTradeHandle(ServerClient client, string data)
        {
            string dataToSend = "";

            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[1]))
            {
                dataToSend = "│SentTrade│Confirm│";

                PlayerUtils.SendTradeRequestToPlayer(client, data);
            }
            else dataToSend = "│SentTrade│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public void SendBarterHandle(ServerClient client, string data)
        {
            string dataToSend = "";

            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[1]))
            {
                dataToSend = "│SentBarter│Confirm│";

                PlayerUtils.SendBarterRequestToPlayer(client, data);
            }
            else dataToSend = "│SentBarter│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public void TradeStatusHandle(ServerClient client, string data)
        {
            string username = data.Split('│')[2];
            ServerClient target = null;

            foreach (ServerClient sc in Networking.connectedClients)
            {
                if (sc.username == username)
                {
                    target = sc;
                    break;
                }
            }

            if (target == null) return;

            if (data.StartsWith("TradeStatus│Deal│"))
            {
                Networking.SendData(target, "│SentTrade│Deal│");

                ConsoleUtils.LogToConsole("Trade Done Between [" + target.username + "] And [" + client.username + "]");
            }

            else if (data.StartsWith("TradeStatus│Reject│"))
            {
                Networking.SendData(target, "│SentTrade│Reject│");
            }
        }

        public void BarterStatusHandle(ServerClient client, string data)
        {
            string user = data.Split('│')[2];
            ServerClient target = null;

            foreach (ServerClient sc in Networking.connectedClients)
            {
                if (sc.username == user)
                {
                    target = sc;
                    break;
                }

                if (sc.homeTileID == user)
                {
                    target = sc;
                    break;
                }
            }

            if (target == null) return;

            if (data.StartsWith("BarterStatus│Deal│"))
            {
                Networking.SendData(target, "│SentBarter│Deal│");

                ConsoleUtils.LogToConsole("Barter Done Between [" + target.username + "] And [" + client.username + "]");
            }

            else if (data.StartsWith("BarterStatus│Reject│"))
            {
                Networking.SendData(target, "│SentBarter│Reject│");
            }

            else if (data.StartsWith("BarterStatus│Rebarter│"))
            {
                Networking.SendData(target, "│SentBarter│Rebarter│" + client.username + "│" + data.Split('│')[3]);
            }
        }

        public void SpyInfoHandle(ServerClient client, string data)
        {
            string dataToSend;
            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[1]))
            {
                dataToSend = "│SentSpy│Confirm│" + PlayerUtils.GetSpyData(data.Split('│')[1], client);
            }
            else dataToSend = "│SentSpy│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public void FactionManagementHandle(ServerClient client, string data)
        {
            if (data == "FactionManagement│Refresh")
            {
                if (client.faction == null) return;
                else Networking.SendData(client, FactionHandler.GetFactionDetails(client));
            }

            else if (data.StartsWith("FactionManagement│Create│"))
            {
                if (client.faction != null) return;

                string factionName = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(factionName)) return;

                Faction factionToFetch = Server.savedFactions.Find(fetch => fetch.name == factionName);
                if (factionToFetch is null)
                    _factionHandler.CreateFaction(factionName, client);
                else Networking.SendData(client, "FactionManagement│NameInUse");
            }

            else if (data == "FactionManagement│Disband")
            {
                if (client.faction == null) return;

                if (FactionHandler.GetMemberPowers(client.faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                Faction factionToCheck = client.faction;
                _factionHandler.PurgeFaction(factionToCheck);
            }

            else if (data == "FactionManagement│Leave")
            {
                if (client.faction == null) return;

                _factionHandler.RemoveMember(client.faction, client);
            }

            else if (data.StartsWith("FactionManagement│Join│"))
            {
                string factionString = data.Split('│')[2];

                Faction factionToJoin = Server.savedFactions.Find(fetch => fetch.name == factionString);

                if (factionToJoin == null) return;
                else _factionHandler.AddMember(factionToJoin, client);
            }

            else if (data.StartsWith("FactionManagement│AddMember"))
            {
                if (client.faction == null) return;

                if (FactionHandler.GetMemberPowers(client.faction, client) == FactionHandler.MemberRank.Member)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID)) Networking.SendData(client, "PlayerNotConnected│");
                else
                {
                    ServerClient memberToAdd = PlayerUtils.GetPlayerFromTile(tileID);
                    if (memberToAdd.faction != null) Networking.SendData(client, "FactionManagement│AlreadyInFaction");
                    else Networking.SendData(memberToAdd, "FactionManagement│Invite│" + client.faction.name);
                }
            }

            else if (data.StartsWith("FactionManagement│RemoveMember"))
            {
                if (client.faction == null) return;

                if (FactionHandler.GetMemberPowers(client.faction, client) == FactionHandler.MemberRank.Member)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID))
                {
                    Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.faction.name);
                    ServerClient memberToRemove = Server.savedClients.Find(fetch => fetch.homeTileID == tileID);

                    if (memberToRemove.faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToRemove.faction.name != factionToCheck.name) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else _factionHandler.RemoveMember(factionToCheck, memberToRemove);
                }

                else
                {
                    ServerClient memberToRemove = PlayerUtils.GetPlayerFromTile(tileID);

                    if (memberToRemove.faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToRemove.faction != client.faction) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else _factionHandler.RemoveMember(client.faction, memberToRemove);
                }
            }

            else if (data.StartsWith("FactionManagement│PromoteMember"))
            {
                if (client.faction == null) return;

                if (FactionHandler.GetMemberPowers(client.faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID))
                {
                    Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.faction.name);
                    ServerClient memberToPromote = Server.savedClients.Find(fetch => fetch.homeTileID == tileID);

                    if (memberToPromote.faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToPromote.faction.name != factionToCheck.name) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else _factionHandler.PromoteMember(factionToCheck, memberToPromote);
                }

                else
                {
                    ServerClient memberToPromote = PlayerUtils.GetPlayerFromTile(tileID);

                    if (memberToPromote.faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToPromote.faction != client.faction) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else _factionHandler.PromoteMember(client.faction, memberToPromote);
                }
            }

            else if (data.StartsWith("FactionManagement│DemoteMember"))
            {
                if (client.faction == null) return;

                if (FactionHandler.GetMemberPowers(client.faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID))
                {
                    Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.faction.name);
                    ServerClient memberToDemote = Server.savedClients.Find(fetch => fetch.homeTileID == tileID);

                    if (memberToDemote.faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToDemote.faction.name != factionToCheck.name) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else _factionHandler.DemoteMember(factionToCheck, memberToDemote);
                }

                else
                {
                    ServerClient memberToDemote = PlayerUtils.GetPlayerFromTile(tileID);

                    if (memberToDemote.faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToDemote.faction != client.faction) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else _factionHandler.DemoteMember(client.faction, memberToDemote);
                }
            }

            else if (data.StartsWith("FactionManagement│BuildStructure"))
            {
                if (client.faction == null) return;

                if (FactionHandler.GetMemberPowers(client.faction, client) == FactionHandler.MemberRank.Member)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];
                string structureID = data.Split('│')[3];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (string.IsNullOrWhiteSpace(structureID)) return;

                _factionBuildingHandler.BuildStructure(client.faction, tileID, structureID);
            }

            else if (data.StartsWith("FactionManagement│DestroyStructure"))
            {
                if (client.faction == null) return;

                if (FactionHandler.GetMemberPowers(client.faction, client) == FactionHandler.MemberRank.Member)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                _factionBuildingHandler.DestroyStructure(client.faction, tileID);
            }

            else if (data.StartsWith("FactionManagement│Silo"))
            {
                if (client.faction == null) return;

                if (client.faction.factionStructures.Find(fetch => fetch is FactionSilo) is not FactionSilo siloToFind)
                    return;

                string siloID = data.Split('│')[3];

                if (data.StartsWith("FactionManagement│Silo│Check"))
                {
                    if (string.IsNullOrWhiteSpace(siloID)) return;

                    Networking.SendData(client, FactionSiloHandler.GetSiloContents(client.faction, siloID));
                }

                if (data.StartsWith("FactionManagement│Silo│Deposit"))
                {
                    string items = data.Split('│')[4];

                    if (string.IsNullOrWhiteSpace(siloID)) return;

                    if (string.IsNullOrWhiteSpace(items)) return;

                    _factionSiloHandler.DepositIntoSilo(client.faction, siloID, items);
                }

                if (data.StartsWith("FactionManagement│Silo│Withdraw"))
                {
                    string itemID = data.Split('│')[4];

                    if (string.IsNullOrWhiteSpace(siloID)) return;

                    if (string.IsNullOrWhiteSpace(itemID)) return;

                    _factionSiloHandler.WithdrawFromSilo(client.faction, siloID, itemID, client);
                }
            }

            else if (data.StartsWith("FactionManagement│Bank"))
            {
                if (client.faction == null)
                    return;

                if (client.faction.factionStructures.Find(fetch => fetch is FactionBank) is not FactionBank bankToFind)
                    return;

                if (data.StartsWith("FactionManagement│Bank│Refresh"))
                {
                    _factionBankHandler.RefreshMembersBankDetails(client.faction);
                }

                else if (data.StartsWith("FactionManagement│Bank│Deposit"))
                {
                    int quantity = int.Parse(data.Split('│')[3]);

                    if (quantity == 0)
                        return;
                        
                    _factionBankHandler.DepositMoney(client.faction, quantity);
                }

                else if (data.StartsWith("FactionManagement│Bank│Withdraw"))
                {
                    int quantity = int.Parse(data.Split('│')[3]);

                    if (quantity == 0)
                        return;

                    _factionBankHandler.WithdrawMoney(client.faction, quantity, client);
                }
            }
        }
    }
}