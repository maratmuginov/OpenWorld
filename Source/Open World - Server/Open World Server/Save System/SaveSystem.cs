using OpenWorldServer.Models;
using OpenWorldServer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OpenWorldServer
{
    public class SaveSystem
    {
        private readonly SaveFileProvider _saveConfig;

        public SaveSystem(SaveFileProvider saveConfig)
        {
            _saveConfig = saveConfig;
        }

        public void SaveBans(Dictionary<string, string> bans)
        {
            string filePath = _saveConfig.GetBansFilePath();
            using var fileStream = File.Create(filePath);
            JsonSerializer.Serialize(fileStream, bans);
        }

        public Dictionary<string, string> LoadBans()
        {
            string filePath = _saveConfig.GetBansFilePath();
            using var fileStream = File.OpenRead(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(fileStream);
        }

        public void SavePlayer(ServerClient playerToSave)
        {
            string filePath = Server.playersFolderPath + playerToSave.username + ".data";

            try
            {
                Directory.CreateDirectory(Server.playersFolderPath);

                using var fileStream = File.OpenWrite(filePath);

                JsonSerializer.Serialize(fileStream, playerToSave);

                fileStream.Flush();
                fileStream.Close();
            }
            catch { }
        }

        public void LoadPlayer(string path)
        {
            try
            {
                using var fileStream = File.OpenRead(path);

                var playerToLoad = JsonSerializer.Deserialize<ServerClient>(fileStream);

                fileStream.Close();

                if (playerToLoad == null)
                    return;

                if (!string.IsNullOrWhiteSpace(playerToLoad.homeTileID))
                {
                    try { Server.savedSettlements.Add(playerToLoad.homeTileID, new List<string>() { playerToLoad.username }); }
                    catch
                    {
                        playerToLoad.homeTileID = null;
                        SavePlayer(playerToLoad);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.username + " Is Using A Cloned Entry! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                if (playerToLoad.faction != null)
                {
                    Faction factionToFech = Server.savedFactions.Find(fetch => fetch.name == playerToLoad.faction.name);
                    if (factionToFech == null)
                    {
                        playerToLoad.faction = null;
                        SavePlayer(playerToLoad);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.username + " Is Using A Missing Faction! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                Server.savedClients.Add(playerToLoad);
            }

            catch { }
        }

        public void SaveFaction(Faction factionToSave)
        {
            string factionSavePath = Server.factionsFolderPath + Path.DirectorySeparatorChar + factionToSave.name + ".bin";

            if (factionToSave.members.Count > 1)
            {
                var orderedDictionary = factionToSave.members.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                factionToSave.members = orderedDictionary;
            }

            using var fileStream = File.OpenWrite(factionSavePath);
            JsonSerializer.Serialize(fileStream, factionToSave);

            fileStream.Flush();
            fileStream.Close();

            if (!Server.savedFactions.Contains(factionToSave))
                Server.savedFactions.Add(factionToSave);
        }

        public IReadOnlyList<Faction> LoadFactions(string[] factionFilePaths)
        {
            int failedToLoadFactions = 0;

            var factions = new List<Faction>();

            foreach (string factionFilePath in factionFilePaths)
            {
                try
                {
                    Faction faction = LoadFaction(factionFilePath);
                    factions.Add(faction);
                }
                catch
                { 
                    failedToLoadFactions++;
                }
            }

            DebugInfoService.PrintLoadedFactions(failedToLoadFactions);

            return factions;
        }

        private static Faction LoadFaction(string factionFilePath)
        {
            using var fileStream = File.OpenRead(factionFilePath);
            return JsonSerializer.Deserialize<Faction>(fileStream);
        }
    }
}
