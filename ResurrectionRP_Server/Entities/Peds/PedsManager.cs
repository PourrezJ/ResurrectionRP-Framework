using Newtonsoft.Json;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using System.Collections.Generic;
using AltV.Net.Async;

namespace ResurrectionRP_Server.Entities.Peds
{
    public class PedsManager
    {
        public static List<Ped> NPCList = new List<Ped>();

        public PedsManager()
        {

            AltAsync.OnClient("Ped_Interact", InteractPed);
            AltAsync.OnClient("Ped_SecondaryInteract", SecondaryInteractPed);

        }



        public static async Task InteractPed(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;
            if (GameMode.Instance.IsDebug)
                Alt.Server.LogInfo("PedsManager | Interaction with Ped");

            Ped npc = GetNPCbyID(int.Parse(args[0]+""));
            if (npc != null || npc?.NpcInteractCallBack != null)
            {
                await npc.NpcInteractCallBack?.Invoke(client, npc);
            }
        }

        public static async Task SecondaryInteractPed(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;
            if (GameMode.Instance.IsDebug)
                Alt.Server.LogInfo("PedsManager | Interaction Secondary with Ped");
            Ped npc = GetNPCbyID(int.Parse(args[0] + ""));
            if (npc != null || npc?.NpcInteractCallBack != null)
            {
                await npc.NpcSecInteractCallBack?.Invoke(client, npc);
            }
        }

        public static Ped GetNPCbyID(int id)
        {
            return NPCList.Find(x => x.ID == id) ?? null;
        }
    }
}
