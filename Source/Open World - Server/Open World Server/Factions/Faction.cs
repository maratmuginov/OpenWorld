using System.Collections.Generic;

namespace OpenWorldServer
{
    public class Faction
    {
        public string name = "";
        public int wealth = 0;
        public Dictionary<ServerClient, FactionHandler.MemberRank> members = new();
        public List<FactionStructure> factionStructures = new();
    }
}