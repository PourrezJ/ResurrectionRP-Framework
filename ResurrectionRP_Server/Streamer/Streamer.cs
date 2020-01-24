using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Blips = ResurrectionRP_Server.Entities.Blips.Blips;
using ResurrectionRP_Server.Entities.Peds;

namespace ResurrectionRP_Server.Streamer
{
    public static partial class Streamer
    {
        public static ConcurrentDictionary<int, dynamic> ListStaticEntities = new ConcurrentDictionary<int, dynamic>();

        public static int StaticEntityNumber { get; internal set; }

        public static void Init()
        {
            AltNetworking.Configure(options =>
            {
                
                if (!string.IsNullOrEmpty(Config.GetSetting<string>("StreamerIP")))
                    options.Ip = Config.GetSetting<string>("StreamerIP");
                    
                //options.Ip = Utils.Util.GetIPAddress();
                options.Port = 46429;
            });

            AltNetworking.OnEntityStreamIn = OnEntityStreamIn;
            AltNetworking.OnEntityStreamOut = OnEntityStreamOut;
        }

        private static void OnEntityStreamIn(INetworkingEntity entity, INetworkingClient client)
        {
            /*
            Ped ped = Ped.GetNPCbyID((int)entity.Id);
            if (ped != null)
            {
                if (ped.Owner == null)
                {
                    var player = TokenToPlayer(client.Token);
                    if (player != null)
                    {
                        lock (player)
                        {
                            ped.TaskWanderStandard(true);
                        }
                    }
                }
            }*/
        }

        private static void OnEntityStreamOut(INetworkingEntity entity, INetworkingClient client)
        {
            Ped ped = Ped.GetNPCbyID(entity.Id);
            if (ped != null)
            {
                if (ped.Owner != null) ped.Owner = null;
            }
        }

        public static int AddStaticEntityBlip(Blips blip)
        {
            ListStaticEntities.TryAdd(blip.id, blip.export());

            if (GameMode.PlayerList.Count > 0)
                Alt.EmitAllClients("createStaticEntity", ListStaticEntities[blip.id]);

            return blip.id;
        }

        public static void UpdateStaticEntityBlip(Blips blip)
        {
            ListStaticEntities[blip.id] = blip.export();

            Alt.EmitAllClients("deleteStaticEntity", blip.id, (int)blip.type);
            Alt.EmitAllClients("createStaticEntity", blip.export());
        }

        public static void DestroyStaticEntityBlip(Blips blip)
        {
            Alt.EmitAllClients("deleteStaticEntity", blip.id, (int)blip.type);

            ListStaticEntities.TryRemove(blip.id, out _);
        }

        public static void LoadStreamPlayer(IPlayer player)
        {
            foreach(KeyValuePair<int, object> item in ListStaticEntities)
            {
                if (!player.Exists)
                    return;
                player.Emit("createStaticEntity", item.Value);
            }
        }
    }
}
