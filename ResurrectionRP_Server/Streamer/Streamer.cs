using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using WorldObject = ResurrectionRP_Server.Entities.Objects.WorldObject;
using TextLabel = ResurrectionRP_Server.Streamer.Data.TextLabel;
using Blips = ResurrectionRP_Server.Entities.Blips.Blips;
using AltV.Net.Data;

namespace ResurrectionRP_Server.Streamer
{
    public static partial class Streamer
    {
        public static int EntityNumber = 0;
        public static ConcurrentDictionary<int, INetworkingEntity> ListEntities = new ConcurrentDictionary<int, INetworkingEntity>();

        public static int StaticEntityNumber = 0;
        public static ConcurrentDictionary<int, dynamic> ListStaticEntities = new ConcurrentDictionary<int, dynamic>();

        public static void Init()
        {
            try
            {
                AltNetworking.Configure(options =>
                {
                    if (!string.IsNullOrEmpty(Config.GetSetting<string>("StreamerIP")))
                        options.Ip = Config.GetSetting<string>("StreamerIP");

                    options.Port = 46429;
                });

                AltNetworking.OnEntityStreamIn = (entity, client) =>
                {
                };

                AltNetworking.OnEntityStreamOut = (entity, client) =>
                {
                };
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        public static int AddEntityPed(Entities.Peds.Ped ped, int dimension = GameMode.GlobalDimension)
        {
            INetworkingEntity item = AltNetworking.CreateEntity(ped.Position.ConvertToEntityPosition(), dimension, GameMode.StreamDistance, ped.Export());
            ListEntities.TryAdd(EntityNumber, item);
            return EntityNumber;
        }

        public static WorldObject AddEntityObject(WorldObject data)
        {
            INetworkingEntity item = AltNetworking.CreateEntity(data.Position.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.StreamDistance, data.export());
            
            ListEntities.TryAdd(data.ID, item);
            return data;
        }

        public static WorldObject UpdateEntityObject(WorldObject obj)
        {
            if (ListEntities.ContainsKey(obj.ID))
            {
                INetworkingEntity oitem = ListEntities[obj.ID];
                oitem.SetData("attach", JsonConvert.SerializeObject(obj.Attachment));
            }

            return obj;
        }

        public static void DeleteEntityObject(WorldObject data)
        {
            if (ListEntities.ContainsKey(data.ID))
            {
                Alt.EmitAllClients("deleteObject", data.ID);
                AltNetworking.RemoveEntity(ListEntities[data.ID]);
                ListEntities.TryRemove(data.ID, out _);
            }      
        }

        public static TextLabel AddEntityTextLabel(string label, Vector3 pos, int font = 2, int r = 255, int g = 255, int b = 255, int a = 128, int drawDistance = 5, int dimension = GameMode.GlobalDimension)
        {
            var data = new TextLabel(label, font, r, g, b, a, EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), dimension, drawDistance, data.export());
            ListEntities.TryAdd(data.id, item);
            return data;
        }

        public static int UpdateEntityTextLabel(int entityid, string label)
        {
            ListEntities[entityid].SetData("text", label);
            return entityid;
        }

        public static int DestroyEntity(int entityid)
        {
            if (ListEntities.ContainsKey(entityid))
            {
                INetworkingEntity entity;
                if (ListEntities.TryRemove(entityid, out entity))
                    AltNetworking.RemoveEntity(entity);
            }
  
            return 0;
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
