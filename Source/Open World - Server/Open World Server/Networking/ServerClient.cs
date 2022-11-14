using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace OpenWorldServer
{
    public class ServerClient
    {
        //Security
        [JsonIgnore] public TcpClient tcp;
        [JsonIgnore] public bool disconnectFlag;
        [JsonIgnore] public bool eventShielded;
        [JsonIgnore] public bool inRTSE;
        [JsonIgnore] public ServerClient inRtsActionWith;

        public ServerClient(TcpClient userSocket)
        {
            tcp = userSocket;
        }

        public string username = "";
        public string password = "";
        public bool isAdmin = false;
        public bool toWipe = false;

        //Relevant Data
        public string homeTileID;
        public List<string> giftString = new();
        public List<string> tradeString = new();
        public Faction faction;

        //Wealth Data
        public int pawnCount;
        public float wealth;

        //Variables Data
        public bool isImmunized = false;
    }
}
