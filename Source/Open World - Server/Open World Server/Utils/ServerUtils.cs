﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace OpenWorldServer
{
    public static class ServerUtils
    {
        public static void SetCulture()
        {
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleUtils.LogToConsole("Using Culture Info: [" + CultureInfo.CurrentCulture + "]");

            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
        }

        public static void SetPaths()
        {
            Server.mainFolderPath = AppDomain.CurrentDomain.BaseDirectory;

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Server Startup:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.LogToConsole("Base Directory At: [" + Server.mainFolderPath + "]");

            Server.logFolderPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "Logs";
            Server.serverSettingsPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "Server Settings.txt";
            Server.worldSettingsPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "World Settings.txt";
            Server.playersFolderPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "Players";
            Server.enforcedModsFolderPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "Enforced Mods";
            Server.whitelistedModsFolderPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "Whitelisted Mods";
            Server.blacklistedModsFolderPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "Blacklisted Mods";
            Server.whitelistedUsersPath = Server.mainFolderPath + Path.DirectorySeparatorChar + "Whitelisted Players.txt";
        }

        public static void CheckServerVersion()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Version Check:");
            Console.ForegroundColor = ConsoleColor.White;

            string latestVersion = "";

            try
            {
                WebClient wc = new WebClient();
                latestVersion = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                latestVersion = latestVersion.Split('│')[1].Replace("- Latest Server Version: ", "");
                latestVersion = latestVersion.Remove(0, 1);
                latestVersion = latestVersion.Remove(latestVersion.Count() - 1, 1);
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This is not dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (Server.serverVersion == latestVersion) ConsoleUtils.LogToConsole("Running Latest Version");
            else ConsoleUtils.LogToConsole("Running Outdated Or Unstable version. Please Update From Github At Earliest Convenience To Prevent Errors");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void CheckSettingsFile()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Settings Check:");
            Console.ForegroundColor = ConsoleColor.White;

            if (File.Exists(Server.serverSettingsPath))
            {
                string[] settings = File.ReadAllLines(Server.serverSettingsPath);

                foreach(string setting in settings)
                {
                    if (setting.StartsWith("Server Name: "))
                    {
                        string splitString = setting.Replace("Server Name: ", "");
                        Server.serverName = splitString;
                        continue;
                    }

                    else if (setting.StartsWith("Server Description: "))
                    {
                        string splitString = setting.Replace("Server Description: ", "");
                        Server.serverDescription = splitString;
                        continue;
                    }

                    else if (setting.StartsWith("Server Local IP: "))
                    {
                        string splitString = setting.Replace("Server Local IP: ", "");
                        Networking.localAddress = IPAddress.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Server Port: "))
                    {
                        string splitString = setting.Replace("Server Port: ", "");
                        Networking.serverPort = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Max Players: "))
                    {
                        string splitString = setting.Replace("Max Players: ", "");
                        Server.maxPlayers = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Allow Dev Mode: "))
                    {
                        string splitString = setting.Replace("Allow Dev Mode: ", "");

                        if (splitString == "True") Server.allowDevMode = true;

                        continue;
                    }

                    else if (setting.StartsWith("Use Whitelist: "))
                    {
                        string splitString = setting.Replace("Use Whitelist: ", "");

                        if (splitString == "True") Server.usingWhitelist = true;

                        continue;
                    }

                    else if (setting.StartsWith("Wealth Warning Threshold: "))
                    {
                        string splitString = setting.Replace("Wealth Warning Threshold: ", "");
                        Server.warningWealthThreshold = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Wealth Ban Threshold: "))
                    {
                        string splitString = setting.Replace("Wealth Ban Threshold: ", "");
                        Server.banWealthThreshold = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Use Wealth System: "))
                    {
                        string splitString = setting.Replace("Use Wealth System: ", "");
                        if (splitString == "True")
                        {
                            Server.usingWealthSystem = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.usingWealthSystem = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Idle System: "))
                    {
                        string splitString = setting.Replace("Use Idle System: ", "");
                        if (splitString == "True")
                        {
                            Server.usingIdleTimer = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.usingIdleTimer = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Idle Threshold (days): "))
                    {
                        string splitString = setting.Replace("Idle Threshold (days): ", "");
                        Server.idleTimer = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Use Road System: "))
                    {
                        string splitString = setting.Replace("Use Road System: ", "");
                        if (splitString == "True")
                        {
                            Server.usingRoadSystem = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.usingRoadSystem = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Aggressive Road Mode (WIP): "))
                    {
                        string splitString = setting.Replace("Aggressive Road Mode (WIP): ", "");
                        if (splitString == "True")
                        {
                            Server.aggressiveRoadMode = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.aggressiveRoadMode = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Modlist Match: "))
                    {
                        string splitString = setting.Replace("Use Modlist Match: ", "");
                        if (splitString == "True")
                        {
                            Server.forceModlist = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.forceModlist = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Modlist Config Match (WIP): "))
                    {
                        string splitString = setting.Replace("Use Modlist Config Match (WIP): ", "");
                        if (splitString == "True")
                        {
                            Server.forceModlistConfigs = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.forceModlistConfigs = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Mod Verification: "))
                    {
                        string splitString = setting.Replace("Use Mod Verification: ", "");
                        if (splitString == "True")
                        {
                            Server.usingModVerification = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.usingModVerification = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Chat: "))
                    {
                        string splitString = setting.Replace("Use Chat: ", "");
                        if (splitString == "True")
                        {
                            Server.usingChat = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.usingChat = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Profanity filter: "))
                    {
                        string splitString = setting.Replace("Use Profanity filter: ", "");
                        if (splitString == "True")
                        {
                            Server.usingProfanityFilter = true;
                        }
                        else if (splitString == "False")
                        {
                            Server.usingProfanityFilter = false;
                        }
                        continue;
                    }
                }

                ConsoleUtils.LogToConsole("Loaded Settings File");
            }

            else
            {
                string[] settingsPreset = new string[]
                {
                    "- Server Details -",
                    "Server Name: My Server Name",
                    "Server Description: My Server Description",
                    "Server Local IP: 0.0.0.0",
                    "Server Port: 25555",
                    "Max Players: 300",
                    "Allow Dev Mode: False",
                    "Use Whitelist: False",
                    "",
                    "- Mod System Details -",
                    "Use Modlist Match: True",
                    "Use Modlist Config Match (WIP): False",
                    "Force Mod Verification: False",
                    "",
                    "- Chat System Details -",
                    "Use Chat: True",
                    "Use Profanity filter: True",
                    "",
                    "- Wealth System Details -",
                    "Use Wealth System: False",
                    "Wealth Warning Threshold: 10000",
                    "Wealth Ban Threshold: 100000",
                    "",
                    "- Idle System Details -",
                    "Use Idle System: True",
                    "Idle Threshold (days): 7",
                    "",
                    "- Road System Details -",
                    "Use Road System: True",
                    "Aggressive Road Mode (WIP): False",
                };

                File.WriteAllLines(Server.serverSettingsPath, settingsPreset);

                ConsoleUtils.LogToConsole("Generating Settings File");

                CheckSettingsFile();
            }
        }

        public static void CheckAllAvailableMods(bool newLine)
        {
            if (newLine) Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Mods Check:");
            Console.ForegroundColor = ConsoleColor.White;

            CheckMods();
            CheckWhitelistedMods();
            CheckBlacklistedMods();
        }

        private static void CheckMods()
        {
            List<string> modlist = new List<string>();
            Server.enforcedMods.Clear();

            if (!Directory.Exists(Server.enforcedModsFolderPath))
            {
                Directory.CreateDirectory(Server.enforcedModsFolderPath);
                ConsoleUtils.LogToConsole("No Enforced Mods Folder Found, Generating");
                return;
            }

            else
            {
                string[] modFolders = Directory.GetDirectories(Server.enforcedModsFolderPath);

                if (modFolders.Length == 0)
                {
                    ConsoleUtils.LogToConsole("No Enforced Mods Found, Ignoring");
                    return;
                }

                else
                {
                    int failedToLoadMods = 0;

                    foreach (string modFolder in modFolders)
                    {
                        try
                        {
                            string aboutFilePath = modFolder + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "About.xml";
                            string[] aboutLines = File.ReadAllLines(aboutFilePath);

                            foreach (string line in aboutLines)
                            {
                                if (line.Contains("<name>") && line.Contains("</name>"))
                                {
                                    string modName = line;

                                    string purgeString = modName.Split('<')[0];
                                    modName = modName.Remove(0, purgeString.Count());

                                    modName = modName.Replace("<name>", "");
                                    modName = modName.Replace("</name>", "");

                                    if (modName.Contains("")) modName = modName.Replace("&amp", "&");
                                    if (modName.Contains("")) modName = modName.Replace("&quot", "&");
                                    if (modName.Contains("")) modName = modName.Replace("&lt", "&");

                                    modlist.Add(modName);
                                    break;
                                }
                            }
                        }

                        catch { failedToLoadMods++; }
                    }

                    modlist.Sort();
                    Server.enforcedMods = modlist.ToList();
                    ConsoleUtils.LogToConsole("Loaded [" + Server.enforcedMods.Count() + "] Enforced Mods");

                    if (failedToLoadMods > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Failed to load [" + failedToLoadMods + "] Enforced Mods");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }

        private static void CheckWhitelistedMods()
        {
            List<string> whitelistedModsList = new List<string>();
            Server.whitelistedMods.Clear();

            if (!Directory.Exists(Server.whitelistedModsFolderPath))
            {
                Directory.CreateDirectory(Server.whitelistedModsFolderPath);
                ConsoleUtils.LogToConsole("No Whitelisted Mods Folder Found, Generating");
                return;
            }

            else
            {
                string[] modFolders = Directory.GetDirectories(Server.whitelistedModsFolderPath);

                if (modFolders.Length == 0) ConsoleUtils.LogToConsole("No Whitelisted Mods Found, Ignoring");

                else
                {
                    int failedToLoadMods = 0;

                    foreach (string modFolder in modFolders)
                    {
                        try
                        {
                            string aboutFilePath = modFolder + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "About.xml";
                            string[] aboutLines = File.ReadAllLines(aboutFilePath);

                            foreach (string line in aboutLines)
                            {
                                if (line.Contains("<name>") && line.Contains("</name>"))
                                {
                                    string modName = line;

                                    string purgeString = modName.Split('<')[0];
                                    modName = modName.Remove(0, purgeString.Count());

                                    modName = modName.Replace("<name>", "");
                                    modName = modName.Replace("</name>", "");

                                    if (modName.Contains("")) modName = modName.Replace("&amp", "&");
                                    if (modName.Contains("")) modName = modName.Replace("&quot", "&");
                                    if (modName.Contains("")) modName = modName.Replace("&lt", "&");

                                    whitelistedModsList.Add(modName);
                                    break;
                                }
                            }
                        }

                        catch { failedToLoadMods++; }
                    }

                    whitelistedModsList.Sort();
                    Server.whitelistedMods = whitelistedModsList;
                    ConsoleUtils.LogToConsole("Loaded [" + Server.whitelistedMods.Count() + "] Whitelisted Mods");

                    if (failedToLoadMods > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Failed to load [" + failedToLoadMods + "] Whitelisted Mods");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }

        private static void CheckBlacklistedMods()
        {
            List<string> blacklistedModsList = new List<string>();
            Server.blacklistedMods.Clear();

            if (!Directory.Exists(Server.blacklistedModsFolderPath))
            {
                Directory.CreateDirectory(Server.blacklistedModsFolderPath);
                ConsoleUtils.LogToConsole("No Blacklisted Mods Folder Found, Generating");
                return;
            }

            else
            {
                string[] modFolders = Directory.GetDirectories(Server.blacklistedModsFolderPath);

                if (modFolders.Length == 0) ConsoleUtils.LogToConsole("No Blacklisted Mods Found, Ignoring");
                else
                {
                    int failedToLoadMods = 0;

                    foreach (string modFolder in modFolders)
                    {
                        try
                        {
                            string aboutFilePath = modFolder + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "About.xml";
                            string[] aboutLines = File.ReadAllLines(aboutFilePath);

                            foreach (string line in aboutLines)
                            {
                                if (line.Contains("<name>") && line.Contains("</name>"))
                                {
                                    string modName = line;

                                    string purgeString = modName.Split('<')[0];
                                    modName = modName.Remove(0, purgeString.Count());

                                    modName = modName.Replace("<name>", "");
                                    modName = modName.Replace("</name>", "");

                                    if (modName.Contains("")) modName = modName.Replace("&amp", "&");
                                    if (modName.Contains("")) modName = modName.Replace("&quot", "&");
                                    if (modName.Contains("")) modName = modName.Replace("&lt", "&");

                                    blacklistedModsList.Add(modName);
                                    break;
                                }
                            }
                        }

                        catch { failedToLoadMods++; }
                    }

                    blacklistedModsList.Sort();
                    Server.blacklistedMods = blacklistedModsList;
                    ConsoleUtils.LogToConsole("Loaded [" + Server.blacklistedMods.Count() + "] Blacklisted Mods");

                    if (failedToLoadMods > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Failed to load [" + failedToLoadMods + "] Blacklisted Mods");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }

        public static void CheckWorldFile()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("World Check:");
            Console.ForegroundColor = ConsoleColor.White;

            if (File.Exists(Server.worldSettingsPath))
            {
                string[] settings = File.ReadAllLines(Server.worldSettingsPath);

                foreach (string setting in settings)
                {
                    if (setting.StartsWith("Globe Coverage (0.3, 0.5, 1.0): "))
                    {
                        string splitString = setting.Replace("Globe Coverage (0.3, 0.5, 1.0): ", "");
                        Server.globeCoverage = float.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Seed: "))
                    {
                        string splitString = setting.Replace("Seed: ", "");
                        Server.seed = splitString;
                        continue;
                    }

                    else if (setting.StartsWith("Overall Rainfall (0-6): "))
                    {
                        string splitString = setting.Replace("Overall Rainfall (0-6): ", "");
                        Server.overallRainfall = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Overall Temperature (0-6): "))
                    {
                        string splitString = setting.Replace("Overall Temperature (0-6): ", "");
                        Server.overallTemperature = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Overall Population (0-6): "))
                    {
                        string splitString = setting.Replace("Overall Population (0-6): ", "");
                        Server.overallPopulation = int.Parse(splitString);
                        continue;
                    }
                }

                ConsoleUtils.LogToConsole("Loaded World File");
            }

            else
            {
                string[] settingsPreset = new string[]
{
                    "- World Settings -",
                    "Globe Coverage (0.3, 0.5, 1.0): 0.3",
                    "Seed: Seed",
                    "Overall Rainfall (0-6): 3",
                    "Overall Temperature (0-6): 3",
                    "Overall Population (0-6): 3"
                };

                File.WriteAllLines(Server.worldSettingsPath, settingsPreset);

                ConsoleUtils.LogToConsole("Generating World File");

                CheckWorldFile();
            }
        }

        public static void SendChatMessage(ServerClient client, string data)
        {
            string message = data.Split('│')[2];

            string messageForConsole = "Chat - [" + client.username + "] " + message;

            ConsoleUtils.LogToConsole(messageForConsole);

            Server.chatCache.Add("[" + DateTime.Now + "]" + " │ " + messageForConsole);

            foreach (ServerClient sc in Networking.connectedClients)
            {
                if (sc == client) continue;
                else Networking.SendData(sc, data);
            }
        }

        public static void RefreshClientCount(ServerClient client)
        {
            int count = Networking.connectedClients.Count;

            foreach (ServerClient sc in Networking.connectedClients)
            {
                if (sc == client) continue;

                try { Networking.SendData(sc, "│PlayerCountRefresh│" + count + "│"); }
                catch { continue; }
            }
        }
    }
}