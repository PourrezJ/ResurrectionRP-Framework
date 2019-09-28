using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using System.Collections.Generic;
using static ResurrectionRP_Server.Entities.Players.PlayerHandler;
using ResurrectionRP_Server.Entities.Players.Data;
using System;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server.Entities.Peds
{
    public class PedsManager
    {
        public static List<Ped> NPCList = new List<Ped>();

        public KeyPressedDelegate OnKeyPressedEvent { get; set; }
        public PedsManager()
        {
            OnKeyPressedEvent += OnKeyPressed;
        }

        private void OnKeyPressed(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicle, IPlayer playerDistant, int streamedID)
        {
            if (!client.Exists)
                return;

            if (raycastData.entityType != 1)
                return;

            Ped ped = NPCList.Find(p => p.Position.DistanceTo(raycastData.pos) <= Globals.MAX_INTERACTION_DISTANCE && p.Model == (AltV.Net.Enums.PedModel)raycastData.entityHash);

            if (ped == null)
                return;

            if (ped.Position.DistanceTo(client.Position) > 3)
                return;

            if (Keycode == ConsoleKey.E)
            {
                if (ped.NpcInteractCallBackAsync != null)
                    Task.Run(async()=> await ped.NpcInteractCallBackAsync.Invoke(client, ped));
                else if (ped.NpcInteractCallBackAsync != null)
                    ped.NpcInteractCallBack.Invoke(client, ped);
            }
            else if (Keycode == ConsoleKey.W)
            {
                if (ped.NpcSecInteractCallBackAsync != null)
                    Task.Run(async () => await ped.NpcSecInteractCallBackAsync.Invoke(client, ped));
                else if (ped.NpcSecInteractCallBack != null)
                    ped.NpcSecInteractCallBack.Invoke(client, ped);
            }
        }

        public static Ped GetNPCbyID(int id)
        {
            return NPCList.Find(x => x.ID == id) ?? null;
        }
    }
}
