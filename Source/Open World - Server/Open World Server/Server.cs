using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenWorldServer
{
    public class Server
    {
        //Meta
        public static bool exit = false;

        //Paths
        public static string mainFolderPath;
        public static string serverSettingsPath;
        public static string worldSettingsPath;
        public static string playersFolderPath;
        public static string factionsFolderPath;
        public static string enforcedModsFolderPath;
        public static string whitelistedModsFolderPath;
        public static string blacklistedModsFolderPath;
        public static string whitelistedUsersPath;
        public static string logFolderPath;

        //Player Parameters
        public static List<ServerClient> savedClients = new List<ServerClient>();
        public static Dictionary<string, List<string>> savedSettlements = new Dictionary<string, List<string>>();

        //Server Details
        public static string serverName = "";
        public static string serverDescription = "";
        public static string serverVersion = "v1.4.2";

        //Server Variables
        public static int maxPlayers = 300;
        public static int warningWealthThreshold = 10000;
        public static int banWealthThreshold = 100000;
        public static int idleTimer = 7;

        //Server Booleans
        public static bool usingIdleTimer = false;
        public static bool allowDevMode = false;
        public static bool usingWhitelist = false;
        public static bool usingWealthSystem = false;
        public static bool usingRoadSystem = false;
        public static bool aggressiveRoadMode = false;
        public static bool forceModlist = false;
        public static bool forceModlistConfigs = false;
        public static bool usingModVerification = false;
        public static bool usingChat = false;
        public static bool usingProfanityFilter = false;
        public static bool usingEnforcedDifficulty = false;

        //Server Mods
        public static List<string> enforcedMods = new List<string>();
        public static List<string> whitelistedMods = new List<string>();
        public static List<string> blacklistedMods = new List<string>();

        //Server Lists
        public static List<string> whitelistedUsernames = new List<string>();
        public static List<string> adminList = new List<string>();
        public static List<string> chatCache = new List<string>();
        public static Dictionary<string, string> bannedIPs = new Dictionary<string, string>();
        public static List<Faction> savedFactions = new List<Faction>();

        //World Parameters
        public static float globeCoverage;
        public static string seed;
        public static int overallRainfall;
        public static int overallTemperature;
        public static int overallPopulation;
        public static string latestClientVersion;

        private readonly PlayerUtils _playerUtils;
        private readonly SimpleCommands _simpleCommands;
        private readonly AdvancedCommands _advancedCommands;
        private readonly FactionHandler _factionHandler;
        private readonly Networking _networking;
        private readonly ServerUtils _serverUtils;

        public Server(PlayerUtils playerUtils, SimpleCommands simpleCommands, AdvancedCommands advancedCommands, FactionHandler factionHandler, Networking networking, ServerUtils serverUtils)
        {
            _playerUtils = playerUtils;
            _simpleCommands = simpleCommands;
            _advancedCommands = advancedCommands;
            _factionHandler = factionHandler;
            _networking = networking;
            _serverUtils = serverUtils;
        }

        public async Task StartAsync()
        {
            ServerUtils.SetPaths();
            ServerUtils.SetCulture();

            await CheckServerVersionAsync();
            await CheckRequiredClientVersionAsync();

            ServerUtils.CheckSettingsFile();

            ModHandler.CheckMods(true);
            _factionHandler.CheckFactions(true);
            WorldHandler.CheckWorldFile();
            _playerUtils.CheckAllAvailablePlayers(false);

            Thread networkingThread = new(_networking.ReadyServer)
            {
                IsBackground = true,
                Name = "Networking Thread"
            };
            networkingThread.Start();

            while (!exit)
            {
                ListenForCommands();
            }
        }

        public async Task CheckServerVersionAsync()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Server Version Check:");
            Console.ForegroundColor = ConsoleColor.White;
            try
            {
                string latestVersion = await _serverUtils.GetRequiredServerVersionAsync();
                if (serverVersion == latestVersion)
                    ConsoleUtils.LogToConsole("Running Latest Version");
                else
                    ConsoleUtils.LogToConsole("Running Outdated Or Unstable version. Please Update From Github At Earliest Convenience To Prevent Errors");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This Is Not Dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private async Task CheckRequiredClientVersionAsync()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Client Version Check:");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                latestClientVersion = await _serverUtils.GetRequiredClientVersionAsync();
                ConsoleUtils.LogToConsole($"Listening For Version [{latestClientVersion}]");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This Is Not Dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void ListenForCommands()
        {
            Console.ForegroundColor = ConsoleColor.White;

            string fullCommand = Console.ReadLine();
            string commandBase = fullCommand.Split(' ')[0].ToLower();

            string commandArguments = "";
            if (fullCommand.Contains(' ')) commandArguments = fullCommand.Replace(fullCommand.Split(' ')[0], "").Remove(0, 1);

            Dictionary<string, Action> simpleCommands = new()
            {
                {"help", _simpleCommands.HelpCommand},
                {"settings", _simpleCommands    .SettingsCommand},
                {"modlist", _simpleCommands.ModListCommand},
                {"reload", _simpleCommands.ReloadCommand},
                {"status", _simpleCommands.StatusCommand},
                {"eventlist", _simpleCommands.EventListCommand},
                {"chat", _simpleCommands.ChatCommand},
                {"list", _simpleCommands.ListCommand},
                {"settlements", _simpleCommands.SettlementsCommand},
                {"banlist", _simpleCommands.BanListCommand},
                {"adminlist", _simpleCommands.AdminListCommand},
                {"whitelist", _simpleCommands.WhiteListCommand},
                {"wipe", _simpleCommands.WipeCommand},
                {"clear", _simpleCommands.ClearCommand},
                {"exit", _simpleCommands.ExitCommand}
            };

            Dictionary<string, Action> advancedCommands = new()
            {
                {"say", _advancedCommands.SayCommand},
                {"broadcast", _advancedCommands.BroadcastCommand},
                {"notify", _advancedCommands.NotifyCommand},
                {"invoke", _advancedCommands.InvokeCommand},
                {"plague", _advancedCommands.PlagueCommand},
                {"player", _advancedCommands.PlayerDetailsCommand},
                {"faction", _advancedCommands.FactionDetailsCommand},
                {"kick", _advancedCommands.KickCommand},
                {"ban", _advancedCommands.BanCommand},
                {"pardon", _advancedCommands.PardonCommand},
                {"promote", _advancedCommands.PromoteCommand},
                {"demote", _advancedCommands.DemoteCommand},
                {"giveitem", _advancedCommands.GiveItemCommand},
                {"giveitemall", _advancedCommands.GiveItemAllCommand},
                {"protect", _advancedCommands.ProtectCommand},
                {"deprotect", _advancedCommands.DeprotectCommand},
                {"immunize", _advancedCommands.ImmunizeCommand},
                {"deimmunize", _advancedCommands.DeimmunizeCommand}
            };

            try
            {
                if (simpleCommands.TryGetValue(commandBase, out Action command))
                    command();

                else if (advancedCommands.TryGetValue(commandBase, out Action advancedCommand))
                {
                    AdvancedCommands.commandData = commandArguments;
                    advancedCommand();
                }

                else _simpleCommands.UnknownCommand(commandBase);
            }

            catch 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.WriteWithTime("Command Caught Exception");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}