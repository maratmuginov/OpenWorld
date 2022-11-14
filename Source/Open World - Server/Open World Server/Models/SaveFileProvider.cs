using System.IO;

namespace OpenWorldServer.Models
{
    public class SaveFileProvider
    {
        public SaveFileProvider(string bansFileName)
        {
            BansFileName = bansFileName;
        }

        public string BansFileName { get; }

        public string GetBansFilePath() => Path.Combine(Server.mainFolderPath, BansFileName);

        public string GetPlayerFilePath(string playerName) => Path.Combine(Server.playersFolderPath, $"{playerName}.data"); 
    }
}
