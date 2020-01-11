using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils.Enums;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public class FactionsCommands
    {
        public FactionsCommands()
        {
            Chat.RegisterCmd("addonu", AddPlayerONU);
            Chat.RegisterCmd("addlspd", AddPlayerLSPD);
            Chat.RegisterCmd("addlscustom", AddPlayerLSCustom);
            Chat.RegisterCmd("addgouv", AddPlayerGouv);
            Chat.RegisterCmd("adddock", AddPlayerDock);
            Chat.RegisterCmd("addsheriff", AddPlayerSheriff);
        }

        public void AddPlayerONU(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            FactionManager.EMS.TryAddIntoFaction(client, 4);
        }

        public void AddPlayerLSPD(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            FactionManager.Lspd.TryAddIntoFaction(client, 4);
        }

        public void AddPlayerLSCustom(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            FactionManager.LSCustom.TryAddIntoFaction(client, 2);
        }

        public void AddPlayerGouv(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            FactionManager.Gouvernement.TryAddIntoFaction(client, 7);
        }

        public void AddPlayerDock(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            FactionManager.Dock.TryAddIntoFaction(client, 5);
        }

        public void AddPlayerSheriff(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            FactionManager.Sheriff.TryAddIntoFaction(client, 6);
        }
    }
}

