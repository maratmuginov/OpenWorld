using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenWorldServer
{
    public class FactionHandler
    {
        private readonly SaveSystem _saveSystem;
        private readonly PlayerUtils _playerUtils;
        public FactionHandler(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
        }

        public enum MemberRank { Member, Moderator, Leader }

        public void CheckFactions(bool newLine)
        {
            if (newLine) Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Factions Check:");
            Console.ForegroundColor = ConsoleColor.White;

            if (!Directory.Exists(Server.factionsFolderPath))
            {
                Directory.CreateDirectory(Server.factionsFolderPath);
                ConsoleUtils.LogToConsole("No Factions Folder Found, Generating");
            }

            else
            {
                string[] factionFiles = Directory.GetFiles(Server.factionsFolderPath);

                if (factionFiles.Length == 0)
                {
                    ConsoleUtils.LogToConsole("No Factions Found, Ignoring");
                    return;
                }
                    
                var factions = _saveSystem.LoadFactions(factionFiles);

                foreach (var factionToLoad in factions)
                {
                    if (factionToLoad.members.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.WriteWithTime("Faction Had 0 Members, Removing");
                        Console.ForegroundColor = ConsoleColor.White;

                        DisbandFaction(factionToLoad);
                        continue;
                    }

                    Faction factionToFetch = Server.savedFactions.Find(fetch => fetch.name == factionToLoad.name);
                    if (factionToFetch == null) Server.savedFactions.Add(factionToLoad);
                }
            }
        }

        public void CreateFaction(string factionName, ServerClient factionLeader)
        {
            Faction newFaction = new Faction();
            newFaction.name = factionName;
            newFaction.wealth = 0;
            newFaction.members.Add(factionLeader, MemberRank.Leader);
            _saveSystem.SaveFaction(newFaction);

            factionLeader.faction = newFaction;

            ServerClient clientToSave = Server.savedClients.Find(fetch => fetch.username == factionLeader.username);
            clientToSave.faction = newFaction;
            _saveSystem.SavePlayer(clientToSave);

            Networking.SendData(factionLeader, "FactionManagement│Created");

            Networking.SendData(factionLeader, GetFactionDetails(factionLeader));
        }

        public static void DisbandFaction(Faction factionToDisband)
        {
            Server.savedFactions.Remove(factionToDisband);

            string factionSavePath = Server.factionsFolderPath + Path.DirectorySeparatorChar + factionToDisband.name + ".bin";
            File.Delete(factionSavePath);
        }

        public static string GetFactionDetails(ServerClient client)
        {
            string dataToSend = "FactionManagement│Details│";

            if (client.faction == null) return dataToSend;

            else
            {
                Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.faction.name);

                dataToSend += factionToCheck.name + "│";

                Dictionary<ServerClient, MemberRank> members = factionToCheck.members;
                foreach (KeyValuePair<ServerClient, MemberRank> member in members)
                {
                    dataToSend += member.Key.username + ":" + (int)member.Value + "»";
                }

                return dataToSend;
            }
        }

        public void AddMember(Faction faction, ServerClient memberToAdd)
        {
            faction.members.Add(memberToAdd, MemberRank.Member);
            _saveSystem.SaveFaction(faction);

            ServerClient connected = Networking.connectedClients.Find(fetch => fetch.username == memberToAdd.username);
            if (connected != null)
            {
                connected.faction = faction;
            }

            ServerClient saved = Server.savedClients.Find(fetch => fetch.username == memberToAdd.username);
            if (saved != null)
            {
                saved.faction = faction;
                _saveSystem.SavePlayer(saved);
            }

            UpdateAllPlayerDetailsInFaction(faction);
        }

        public void RemoveMember(Faction faction, ServerClient memberToRemove)
        {
            foreach (KeyValuePair<ServerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.username == memberToRemove.username)
                {
                    faction.members.Remove(pair.Key);
                    break;
                }
            }

            ServerClient connected = Networking.connectedClients.Find(fetch => fetch.username == memberToRemove.username);
            if (connected != null)
            {
                connected.faction = null;
                Networking.SendData(connected, GetFactionDetails(connected));
            }

            ServerClient saved = Server.savedClients.Find(fetch => fetch.username == memberToRemove.username);
            if (saved != null)
            {
                saved.faction = null;
                _saveSystem.SavePlayer(saved);
            }

            if (faction.members.Count > 0)
            {
                _saveSystem.SaveFaction(faction);
                UpdateAllPlayerDetailsInFaction(faction);
            }
            else DisbandFaction(faction);
        }

        public void PurgeFaction(Faction faction)
        {
            ServerClient[] dummyfactionMembers = faction.members.Keys.ToArray();

            foreach(ServerClient dummy in dummyfactionMembers)
            {
                ServerClient connected = Networking.connectedClients.Find(fetch => fetch.username == dummy.username);
                if (connected != null)
                {
                    connected.faction = null;
                    Networking.SendData(connected, GetFactionDetails(connected));
                }

                ServerClient saved = Server.savedClients.Find(fetch => fetch.username == dummy.username);
                if (saved != null)
                {
                    saved.faction = null;
                    _saveSystem.SavePlayer(saved);
                }
            }

            DisbandFaction(faction);
        }

        public void PromoteMember(Faction faction, ServerClient memberToPromote)
        {
            ServerClient toPromote = null;
            MemberRank previousRank = 0;

            foreach (KeyValuePair<ServerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.username == memberToPromote.username)
                {
                    toPromote = pair.Key;
                    previousRank = pair.Value;
                    break;
                }
            }

            if (previousRank > 0) return;

            faction.members.Remove(toPromote);

            faction.members.Add(memberToPromote, MemberRank.Moderator);

            _saveSystem.SaveFaction(faction);
            UpdateAllPlayerDetailsInFaction(faction);
        }

        public void DemoteMember(Faction faction, ServerClient memberToPromote)
        {
            ServerClient toDemote = null;
            MemberRank previousRank = 0;

            foreach (KeyValuePair<ServerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.username == memberToPromote.username)
                {
                    toDemote = pair.Key;
                    previousRank = pair.Value;
                    break;
                }
            }

            if (previousRank == 0) return;

            faction.members.Remove(toDemote);

            faction.members.Add(memberToPromote, MemberRank.Member);

            _saveSystem.SaveFaction(faction);
            UpdateAllPlayerDetailsInFaction(faction);
        }

        public static void UpdateAllPlayerDetailsInFaction(Faction faction)
        {
            ServerClient[] dummyfactionMembers = faction.members.Keys.ToArray();

            foreach (ServerClient dummy in dummyfactionMembers)
            {
                ServerClient connected = Networking.connectedClients.Find(fetch => fetch.username == dummy.username);
                if (connected != null)
                {
                    Networking.SendData(connected, GetFactionDetails(connected));
                }
            }
        }

        public static MemberRank GetMemberPowers(Faction faction, ServerClient memberToCheck)
        {
            Dictionary<ServerClient, MemberRank> members = faction.members;
            foreach (KeyValuePair<ServerClient, MemberRank> pair in members)
            {
                if (pair.Key.username == memberToCheck.username)
                {
                    return pair.Value;
                }
            }

            return MemberRank.Member;
        }
    }
}