using System;

namespace OpenWorldServer.Services
{
    public class DebugInfoService
    {
        public static void PrintLoadedFactions(int failedToLoadFactions)
        {
            ConsoleUtils.LogToConsole("Loaded [" + Server.savedFactions.Count + "] Factions");

            if (failedToLoadFactions > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.LogToConsole("Failed to load [" + failedToLoadFactions + "] Factions");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

    }
}
