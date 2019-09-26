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
    public partial class Streamer
    {
        public int EntityNumber = 0;
        public ConcurrentDictionary<int, INetworkingEntity> ListEntities = new ConcurrentDictionary<int, INetworkingEntity>();

        public int StaticEntityNumber = 0;
        public ConcurrentDictionary<int, dynamic> ListStaticEntities = new ConcurrentDictionary<int, dynamic>();

        public Streamer()
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

        public int AddEntityPed(Entities.Peds.Ped ped, int dimension = GameMode.GlobalDimension)
        {
            INetworkingEntity item = AltNetworking.CreateEntity(ped.Position.ConvertToEntityPosition(), dimension, GameMode.Instance.StreamDistance, ped.export());
            ListEntities.TryAdd(EntityNumber, item);
            return EntityNumber;
        }

        public WorldObject AddEntityObject(WorldObject data)
        {
            INetworkingEntity item = AltNetworking.CreateEntity(data.Position.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, data.export());
            ListEntities.TryAdd(data.ID, item);
            return data;
        }

        public WorldObject UpdateEntityObject(WorldObject obj)
        {
            INetworkingEntity oitem = this.ListEntities[obj.ID];
            oitem.SetData("attach",JsonConvert.SerializeObject( obj.Attachment ) );

            return obj;
        }

        public void DeleteEntityObject(WorldObject data)
        {
            Alt.EmitAllClients("deleteObject", data.ID);
            AltNetworking.RemoveEntity(ListEntities[data.ID]);
            ListEntities.TryRemove(data.ID, out _);
        }

        public TextLabel AddEntityTextLabel(string label, Vector3 pos, int font = 1, int r = 255, int g = 255, int b = 255, int a = 255, int drawDistance = 20, int dimension = GameMode.GlobalDimension)
        {
            var data = new TextLabel(label, font, r, g, b, a, EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), dimension, drawDistance, data.export());
            ListEntities.TryAdd(data.id, item);
            return data;
        }

        public int UpdateEntityTextLabel(int entityid, string label)
        {
            ListEntities[entityid].SetData("text", label);
            return entityid;
        }

        public int DestroyEntity(int entityid)
        {
            AltNetworking.RemoveEntity(ListEntities[entityid]);
            ListEntities.TryRemove(entityid, out _);
            return 0;
        }

        public int AddStaticEntityBlip(Blips blip)
        {
            ListStaticEntities.TryAdd(blip.id, blip.export());

            if (GameMode.Instance.PlayerList.Count > 0)
                Alt.EmitAllClients("createStaticEntity", ListStaticEntities[blip.id]);

            return blip.id;
        }

        public void UpdateStaticEntityBlip(Blips blip)
        {
            ListStaticEntities[blip.id] = blip.export();

            Alt.EmitAllClients("deleteStaticEntity", blip.id, (int)blip.type);
            Alt.EmitAllClients("createStaticEntity", blip.export());
        }

        public void DestroyStaticEntityBlip(Blips blip)
        {
            Alt.EmitAllClients("deleteStaticEntity", blip.id, (int)blip.type);

            ListStaticEntities.TryRemove(blip.id, out _);
        }

        public void LoadStreamPlayer(IPlayer player)
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
