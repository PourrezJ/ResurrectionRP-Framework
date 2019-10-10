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
            Chat.RegisterCmd("addrebelle", AddPlayerRebelle);
            Chat.RegisterCmd("addlscustom", AddPlayerLSCustom);
            Chat.RegisterCmd("addgouv", AddPlayerGouv);
            Chat.RegisterCmd("adddock", AddPlayerDock);
            Chat.RegisterCmd("addsheriff", AddPlayerSheriff);
        }

        public async Task AddPlayerONU(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await FactionManager.Onu.TryAddIntoFaction(client, 4);
        }

        public async Task AddPlayerLSPD(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await FactionManager.Lspd.TryAddIntoFaction(client, 4);
        }
        
        public async Task AddPlayerRebelle(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await FactionManager.Rebelle.TryAddIntoFaction(client, 3);
        }

        public async Task AddPlayerLSCustom(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await FactionManager.LSCustom.TryAddIntoFaction(client, 2);
        }

        public async Task AddPlayerGouv(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await FactionManager.Gouvernement.TryAddIntoFaction(client, 7);
        }
        
        public async Task AddPlayerDock(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await FactionManager.Dock.TryAddIntoFaction(client, 5);
        }
        
        public async Task AddPlayerSheriff(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await FactionManager.Nordiste.TryAddIntoFaction(client, 6);
        }
    }
}

