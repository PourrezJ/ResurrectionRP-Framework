using AltV.Net;
using AltV.Net.Async;
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
            //Chat.RegisterCmd("addrebelle", AddPlayerRebelle);
            Chat.RegisterCmd("addlscustom", AddPlayerLSCustom);
            Chat.RegisterCmd("addgouv", AddPlayerGouv);
            //Chat.RegisterCmd("adddock", AddPlayerDock);
            //Chat.RegisterCmd("addsheriff", AddPlayerSheriff);
        }

        public async Task AddPlayerONU(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await GameMode.Instance.FactionManager?.Onu.TryAddIntoFaction(client, 4);
        }

        public async Task AddPlayerLSPD(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await GameMode.Instance.FactionManager?.Lspd.TryAddIntoFaction(client, 4);
        }
        /*
        public void AddPlayerRebelle(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            Task.Run(async () => await GameMode.Instance.FactionManager?.Rebelle.TryAddIntoFaction(client, 3));
        }*/

        public async Task AddPlayerLSCustom(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await GameMode.Instance.FactionManager?.LSCustom.TryAddIntoFaction(client, 2);
        }

        public async Task AddPlayerGouv(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            await GameMode.Instance.FactionManager?.Gouvernement.TryAddIntoFaction(client, 7);
        }
        /*
        public void AddPlayerDock(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            Task.Run(async () => await GameMode.Instance.FactionManager?.Dock.TryAddIntoFaction(client, 5));
        }
        
        public void AddPlayerSheriff(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;
            Task.Run(async () => await GameMode.Instance.FactionManager?.Nordiste.TryAddIntoFaction(client, 6));
        }*/
    }
}

