﻿using System.Linq;
using System.Threading;

namespace OpenWorldServer
{
    public class FactionBankHandler
    {
        private readonly SaveSystem _saveSystem;

        public void TickBank()
        {
            while (true)
            {
                Thread.Sleep(600000);

                Faction[] allFactions = Server.savedFactions.ToArray();
                foreach (Faction faction in allFactions)
                {
                    FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

                    if (bankToFind == null) continue;
                    else
                    {
                        bankToFind.depositedSilver += 100;
                        _saveSystem.SaveFaction(faction);
                    }
                }

                ConsoleUtils.LogToConsole("[Factions Bank Tick]");
            }
        }

        public void DepositMoney(Faction faction, int quantity)
        {
            FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

            if (bankToFind == null) return;

            bankToFind.depositedSilver += quantity;

            _saveSystem.SaveFaction(faction);

            RefreshMembersBankDetails(faction);
        }

        public void WithdrawMoney(Faction faction, int quantity, ServerClient client)
        {
            FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

            if (bankToFind == null) return;

            if (bankToFind.depositedSilver - quantity < 0) return;
            else bankToFind.depositedSilver -= quantity;

            Networking.SendData(client, "FactionManagement│Bank│Withdraw" + "│" + quantity);

            _saveSystem.SaveFaction(faction);

            RefreshMembersBankDetails(faction);
        }

        public void RefreshMembersBankDetails(Faction faction)
        {
            FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

            if (bankToFind == null) return;

            ServerClient[] dummyFactionMembers = faction.members.Keys.ToArray();
            foreach(ServerClient dummy in dummyFactionMembers)
            {
                ServerClient connected = Networking.connectedClients.Find(fetch => fetch.username == dummy.username);
                if (connected != null)
                {
                    Networking.SendData(connected, "FactionManagement│Bank│Refresh│" + bankToFind.depositedSilver);
                }
            }
        }
    }
}
