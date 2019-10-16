using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using ResurrectionRP_Server.EventHandlers;
using System.Threading.Tasks;
using System.Numerics;
using ResurrectionRP_Server.Factions;
using Newtonsoft.Json;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server.Models
{
    public class Call
    {
        public int Id;
        public string FactionName;
        public IPlayer Player;
        public DateTime Expire = DateTime.Now.AddMinutes(3);
        public DateTime Date = DateTime.Now;
        public Vector3 Position;
        public string Reason;
        public IPlayer TakenBy = null ;

        public Call(IPlayer client, Vector3 pos , int ID, string FactionName)
        {
            this.Player = client;
            this.Position = pos;
            this.Id = ID;
            this.FactionName = FactionName;
        }


        public void TakeCall(IPlayer client)
        {
            this.TakenBy = client;
            client.Emit("EC_TakeCall", FactionName, Id);
        }

    }

    public class EmergencyCall
    {

        public static int DefaultCallAgainTime = 2; // En Minutes

        private int CallsID = 0;
        public string FactionName = null;

        // Faire un menu lorsqu'on accepte, pour donner une estimation de temps -> Bloquer le joueur pendant ce temps

        public ConcurrentDictionary<int, IPlayer> ConcernedPlayers = new ConcurrentDictionary<int, IPlayer>();
        public ConcurrentDictionary<int, IPlayer> InMissionPlayers = new ConcurrentDictionary<int, IPlayer>();
        public ConcurrentDictionary<int, Call> Calls = new ConcurrentDictionary<int, Call>();

        public int ECBlipSprite = 480;
        public int[] ECBlipColors =   { 49, 77, 25 }; // 0: Call fait 1: Call prit en charge 2: Call prit vu du joueur

        public Banner ECBanner = null;

        public EmergencyCall(string FactionNames)  {
            this.FactionName = FactionNames;
            Events.OnPlayerEmitEmergencyCall += Events_OnPlayerEmitEmergencyCall;
            Events.OnPlayerReleaseEmergencyCall += Events_OnPlayerReleaseEmergencyCall;
            Events.OnPlayerRemoveEmergencyCall += Events_OnPlayerRemoveEmergencyCall;

            Events.OnPlayerChangeStateEmergencyCall += Events_OnPlayerChangeStateEmergencyCall;
            Events.OnPlayerMenuEmergencyCall += Events_OnPlayerMenuEmergencyCall;
        }


        protected bool AddConcernedPlayer(IPlayer player)
        {
            if (GameMode.IsDebug)
                Alt.Server.LogInfo("EmergencyCall | Add player into concernedPlayers " + player.GetPlayerHandler()?.PID + " | " + FactionName);

            if (!player.Exists)
                return false;

            if (this.ConcernedPlayers.ContainsKey(player.Id))
                return true;

            if (this.ConcernedPlayers.TryAdd(player.Id, player))
                return true;

            Alt.Server.LogError("EmergencyCall | Error when trying to add concerned player " + player.GetPlayerHandler()?.PID + " | " + FactionName);

            return false;
        }

        protected bool RemoveConcernedPlayer(IPlayer player)
        {
            if (GameMode.IsDebug)
                Alt.Server.LogInfo("EmergencyCall | Remove player into concernedPlayers " + player.GetPlayerHandler()?.PID + " | " + FactionName);

            if (!player.Exists)
                return false;

            if (this.ConcernedPlayers.ContainsKey(player.Id))
            {
                if (this.ConcernedPlayers.TryRemove(player.Id, out IPlayer data))
                    return true;
                else
                    Alt.Server.LogError("EmergencyCall | Error when trying to remove concerned player " + player.GetPlayerHandler()?.PID + " | " + FactionName);
            }
            return false;

        }


        private void Events_OnPlayerTakeEmergencyCall(IPlayer client, int callid)
        {
            if (!ConcernedPlayers.ContainsKey(client.Id))
                return;

            if (Calls.TryGetValue(callid, out Call call) && call == null )
            {
                Alt.Server.LogError("EmergencyCall | Events_OnPlayerTakeEmergencyCall | Trying to take a call that do not exists");
                return;
            }

            if(call.TakenBy != null )
            {
                client.DisplayHelp("L'appel à déjà été pris !", 10000);
                return;
            }

            call.TakeCall(client);
            InMissionPlayers.TryAdd(client.Id, client);

            foreach (KeyValuePair<int, IPlayer> pl in ConcernedPlayers)
                if (pl.Value == call.TakenBy)
                    pl.Value.Emit("EC_UpdateBlipColor", FactionName, call.Id, ECBlipColors[2]);
                else
                    pl.Value.Emit("EC_UpdateBlipColor", FactionName, call.Id, ECBlipColors[1]);

            client.Emit("SetNotificationMessage", "CHAR_BRYONY", "Centrale", "Information", "Ok, vous venez de prendre l'appel, il est disponible sur votre gps!");
            call.Player.SendNotificationSuccess("Quelqu'un a répondu à votre appel!");


        }

        private void Events_OnPlayerRemoveEmergencyCall(IPlayer client, int callid)
        {
            if (!ConcernedPlayers.ContainsKey(client.Id))
                return;

            if (Calls.TryGetValue(callid, out Call call) && call == null)
            {
                Alt.Server.LogError("EmergencyCall | Events_OnPlayerRemoveEmergencyCall | Trying to Remove a call that do not exists");
                return;
            }

            if(call.TakenBy != null)
            {
                client.Emit("SetNotificationMessage", "CHAR_BRYONY", "Centrale", "Information", "L'appel que vous aviez a été annulé!");
                Events_OnPlayerChangeStateEmergencyCall(call.TakenBy, FactionName, true);
            }

            ReleaseCall(call);
            if (!Calls.TryRemove(call.Id, out Call voided))
                Alt.Server.LogError("EmergencyCall | Erreur lors de la suppression d'un call | Events_OnPlayerRemoveEmergencyCall");

            if (InMissionPlayers.ContainsKey(client.Id))
                InMissionPlayers.Remove(client.Id, out IPlayer voidedd);
        }

        private void Events_OnPlayerReleaseEmergencyCall(IPlayer client, int callid)
        {
            try
            {
                if (!ConcernedPlayers.ContainsKey(client.Id) && !InMissionPlayers.ContainsKey(client.Id))
                    return;

                if (Calls.TryGetValue(callid, out Call call) && call == null)
                {
                    Alt.Server.LogError("EmergencyCall | Events_OnPlayerReleaseEmergencyCall | Trying to release a call that do not exists");
                    return;
                }

                if (call.TakenBy != client)
                    return;

                ReleaseCall(call);

                if (!Calls.TryRemove(call.Id, out _))
                    Alt.Server.LogError("EmergencyCall | Erreur lors de la suppression d'un call | Events_OnPlayerReleaseEmergencyCall");

                if (InMissionPlayers.ContainsKey(client.Id))
                    InMissionPlayers.Remove(client.Id, out _);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError($"[EmergencyCall.Events_OnPlayerReleaseEmergencyCall()] client: {client.Id}, callid: {callid} - {ex}");
            }
        }

        private void Events_OnPlayerEmitEmergencyCall(IPlayer client, string factionName, System.Numerics.Vector3 position, string reason)
        {
            if (FactionName == null || factionName != FactionName || !client.Exists)
                return;

            if(ConcernedPlayers.Count == 0 && !GameMode.IsDebug)
            {
                client.DisplayHelp("Personne n'est disponible ! ", 10000);
                // Callback en cas de 0
                return;
            }

/*            if (ConcernedPlayers.Count == 0 && GameMode.IsDebug)
                AddConcernedPlayer(client);*/

            foreach(KeyValuePair<int, Call> pl in Calls)
            {
                if (pl.Value == null)
                    return;

                if(pl.Value.Player == client)
                {
                    if(pl.Value.Expire > DateTime.Now)
                    {
                        client.DisplayHelp("Vous avez déjà fait un appel trop récemment !", 10000);
                        return;
                    }
                    else 
                    {
                        ReleaseCall(pl.Value);
                        if (!Calls.TryRemove(pl.Value.Id, out Call voided))
                            Alt.Server.LogError("EmergencyCall | Erreur lors de la suppression d'un call | Events_OnPlayerEmitEmergencyCall");
                    }
                }
            }

            Call call = new Call(client, position, CallsID++, FactionName);
            call.Reason = reason;

            foreach (KeyValuePair<int, IPlayer> player in ConcernedPlayers)
                player.Value.Emit("EC_EmitCall", FactionName, call.Id, JsonConvert.SerializeObject( position ), ECBlipSprite, ECBlipColors[0], reason);

            Calls.TryAdd(call.Id, call);
        }

        private Task Events_OnPlayerChangeStateEmergencyCall(IPlayer client, string FactionNames, bool state)
        {
            if (FactionName != FactionNames)
                return Task.CompletedTask;

            if (state) // Vérifier si le joueur est toujours en service après changement état.
            {
                AddConcernedPlayer(client);

                if (InMissionPlayers.ContainsKey(client.Id))
                    InMissionPlayers.Remove(client.Id, out IPlayer voidedd);
            }
            else
            {
                RemoveConcernedPlayer(client);

                if (InMissionPlayers.ContainsKey(client.Id))
                    InMissionPlayers.Remove(client.Id, out IPlayer voidedd);
            }

            return Task.CompletedTask;
        }


        private void Events_OnPlayerMenuEmergencyCall(IPlayer client)
        {
            if (!ConcernedPlayers.ContainsKey(client.Id))
                return;

            Menu menu = null;
            if (ECBanner != null)
                menu = new Menu("ID_Appels", "Centre appels " + FactionName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true, enableBanner: true, banner: ECBanner);
            else
                menu = new Menu("ID_Appels", "Centre appels " + FactionName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);

            menu.ItemSelectCallback = Events_OnPlayerMenuEmergencyCallCallback;

            foreach(KeyValuePair<int, Call> pl in Calls)
            {
                Call call = pl.Value;
                if (call.TakenBy != null || call.Expire < DateTime.Now)
                    continue;
                string title = $"Appel de " + call.Date.ToShortTimeString();
                string desc = "Appel effectué à " + call.Date.ToShortTimeString() + " \nRaison: " + call.Reason;
                MenuItem menuItem = new MenuItem(title, desc, id: "ID_AcceptCall", executeCallback: true, rightLabel: Math.Round(call.Position.DistanceTo(client.Position.ConvertToVector3())/1000, 1) + "km");
                menuItem.SetData("call", pl.Value);
                menu.Add(menuItem);
            }

            if (InMissionPlayers.ContainsKey(client.Id))
                menu.Add(new MenuItem(
                    "Être de nouveau disponible",
                    "Cela vous permettra d'être notifié en cas d'appel!",
                    "ID_Service",
                    true
                ));
            menu.OpenMenu(client);
        }

        private void Events_OnPlayerMenuEmergencyCallCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!client.Exists)
                return;

            switch(menuItem.Id)
            {
                case "ID_Service":
                    if (InMissionPlayers.ContainsKey(client.Id))
                        InMissionPlayers.Remove(client.Id, out IPlayer voidedd);
                    client.Emit("SetNotificationMessage", "CHAR_BRYONY", "Centrale", "Information", "Super, vous êtes de nouveau disponible!");
                    break;
                case "ID_AcceptCall":
                    Events_OnPlayerTakeEmergencyCall(client, (menuItem.GetData("call") as Call).Id );
                    break;
                default:
                    Alt.Server.LogError("Unknown emergencycall menu callback id");
                    break;
            }
            menu.CloseMenu(client);
        }

        private void ReleaseCall(Call call)
        {
            foreach (KeyValuePair<int, IPlayer> pl in ConcernedPlayers)
                pl.Value.Emit("EC_ReleaseCall", FactionName, call.Id);
        }

        private Call GetCallFromPlayer(IPlayer client)
        {

            foreach (KeyValuePair<int, Call> pl in Calls)
                if (pl.Value.Player == client)
                    return pl.Value;
            return null;
       }

    }
}
