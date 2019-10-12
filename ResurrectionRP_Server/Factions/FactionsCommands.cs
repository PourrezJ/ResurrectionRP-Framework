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

        public Task AddPlayerONU(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return Task.CompletedTask;

            FactionManager.Onu.TryAddIntoFaction(client, 4);
            return Task.CompletedTask;
        }

        public Task AddPlayerLSPD(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return Task.CompletedTask;

            FactionManager.Lspd.TryAddIntoFaction(client, 4);
            return Task.CompletedTask;
        }

        public Task AddPlayerRebelle(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return Task.CompletedTask;

            FactionManager.Rebelle.TryAddIntoFaction(client, 3);
            return Task.CompletedTask;
        }

        public Task AddPlayerLSCustom(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return Task.CompletedTask;

            FactionManager.LSCustom.TryAddIntoFaction(client, 2);
            return Task.CompletedTask;
        }

        public Task AddPlayerGouv(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return Task.CompletedTask;

            FactionManager.Gouvernement.TryAddIntoFaction(client, 7);
            return Task.CompletedTask;
        }

        public Task AddPlayerDock(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return Task.CompletedTask;

            FactionManager.Dock.TryAddIntoFaction(client, 5);
            return Task.CompletedTask;
        }

        public Task AddPlayerSheriff(IPlayer client, string[] args)
        {
            if (client.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return Task.CompletedTask;

            FactionManager.Nordiste.TryAddIntoFaction(client, 6);
            return Task.CompletedTask;
        }
    }
}

