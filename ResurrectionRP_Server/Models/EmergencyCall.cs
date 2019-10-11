using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using ResurrectionRP_Server.EventHandlers;
using System.Threading.Tasks;
using System.Numerics;

namespace ResurrectionRP_Server.Models
{
    class Call
    {
        public int Id;
        public IPlayer Player;
        public DateTime Expire = DateTime.Now.AddMinutes(2);
        public Vector3 Position;
        public bool Taken;
        public IPlayer TakenBy;
    }
    class EmergencyCall
    {
        public static int DefaultCallAgainTime = 2; // En Minutes

        // Faire un menu lorsqu'on accepte, pour donner une estimation de temps -> Bloquer le joueur pendant ce temps
        // Si le joueur se déco prévenir la fin de l'appel
        // Si le joueur fait R, fin d'appel

        private string FactionName = null;

        public ConcurrentDictionary<int, IPlayer> ConcernedPlayers = new ConcurrentDictionary<int, IPlayer>();
        public ConcurrentDictionary<int, Call> Calls = new ConcurrentDictionary<int, Call>();

        public int BlipSprite = 0;
        public int[] BlipColors =   { 0, 1, 2 }; // 0: Call fait 1: Call prit en charge 2: Call prit vu du joueur
        

        public EmergencyCall()
        {

            Events.OnPlayerEmitEmergencyCall += Events_OnPlayerEmitEmergencyCall;
            Events.OnPlayerReleaseEmergencyCall += Events_OnPlayerReleaseEmergencyCall;
            Events.OnPlayerRemoveEmergencyCall += Events_OnPlayerRemoveEmergencyCall;
            Events.OnPlayerTakeEmergencyCall += Events_OnPlayerTakeEmergencyCall;

            Events.OnPlayerChangeStateEmergencyCall += Events_OnPlayerChangeStateEmergencyCall;

        }


        protected bool AddConcernedPlayer(IPlayer player)
        {
            if (GameMode.IsDebug)
                Alt.Server.LogInfo("EmergencyCall | Add player into concernedPlayers " + player.GetPlayerHandler()?.PID);

            if (!player.Exists)
                return false;

            if (this.ConcernedPlayers.ContainsKey(player.Id))
                return true;

            if (this.ConcernedPlayers.TryAdd(player.Id, player))
                return true;

            Alt.Server.LogError("EmergencyCall | Error when trying to add concerned player " + player.GetPlayerHandler()?.PID);

            return false;
        }

        protected bool RemoveConcernedPlayer(IPlayer player)
        {
            if (GameMode.IsDebug)
                Alt.Server.LogInfo("EmergencyCall | Remove player into concernedPlayers " + player.GetPlayerHandler()?.PID);

            if (!player.Exists)
                return false;

            if (this.ConcernedPlayers.ContainsKey(player.Id))
            {
                if (this.ConcernedPlayers.TryRemove(player.Id, out IPlayer data))
                    return true;
                else
                    Alt.Server.LogError("EmergencyCall | Error when trying to remove concerned player " + player.GetPlayerHandler()?.PID);
            }
            return false;

        }


        private void Events_OnPlayerTakeEmergencyCall(IPlayer client, string factionName, int call)
        {
            if (FactionName == null || factionName != FactionName)
                return;

            throw new NotImplementedException();
        }

        private void Events_OnPlayerRemoveEmergencyCall(IPlayer client, string factionName, int call)
        {
            if (FactionName == null || factionName != FactionName)
                return;

            throw new NotImplementedException();
        }

        private void Events_OnPlayerReleaseEmergencyCall(IPlayer client, string factionName, int call)
        {
            if (FactionName == null || factionName != FactionName)
                return;

            throw new NotImplementedException();
        }

        private Task Events_OnPlayerEmitEmergencyCall(IPlayer client, string factionName, System.Numerics.Vector3 position, string reason)
        {
            if (FactionName == null || factionName != FactionName || !client.Exists)
                return Task.CompletedTask;

            if(Calls.ContainsKey(client.Id) && Calls[client.Id].Expire > DateTime.Now)
            {
                client.DisplayHelp("Vous avez déjà fait un appel trop récemment !", 10000);
                return Task.CompletedTask;
            }

            throw new NotImplementedException();
        }

        private Task Events_OnPlayerChangeStateEmergencyCall(IPlayer client, string factionName, bool state)
        {
            if (FactionName == null || factionName != FactionName || !client.Exists)
                return Task.CompletedTask;

            if (state) // Vérifier si le joueur est toujours en service après changement état.
                AddConcernedPlayer(client);
            else
                RemoveConcernedPlayer(client);

            return Task.CompletedTask;
        }

    }
}
