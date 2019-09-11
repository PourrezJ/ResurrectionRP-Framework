using AltV.Net.Data;
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
using Object = ResurrectionRP_Server.Streamer.Data.Object;
using TextLabel = ResurrectionRP_Server.Streamer.Data.TextLabel;
using PedType = ResurrectionRP_Server.Streamer.Data.PedType;
using Marker = ResurrectionRP_Server.Streamer.Data.Marker;
using Blips = ResurrectionRP_Server.Entities.Blips.Blips;


namespace ResurrectionRP_Server.Streamer
{
    public partial class Streamer
    {
        public int EntityNumber = 0;
        public ConcurrentDictionary<int,INetworkingEntity> ListEntities = new ConcurrentDictionary<int, INetworkingEntity>();

        public int StaticEntityNumber = 0;
        public ConcurrentDictionary<int, dynamic> ListStaticEntities = new ConcurrentDictionary<int, dynamic>();

        public  Streamer()
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

        public int AddEntityObject(string model, Vector3 pos, int dimension = GameMode.GlobalDimension)
        {
            var data = new Object(model,  EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), dimension, GameMode.Instance.StreamDistance, data.export());
            ListEntities.TryAdd(EntityNumber, item);
            return EntityNumber;
        }

        public TextLabel AddEntityTextLabel(string label, Vector3 pos, int font = 1, int r = 255, int g = 255, int b = 255, int a = 255, int drawDistance = 20, int dimension = GameMode.GlobalDimension)
        {
            var data = new TextLabel(label, font, r, g, b, a, EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), dimension, drawDistance, data.export());
            ListEntities.TryAdd(EntityNumber, item);
            return data;
        }

        public int UpdateEntityTextLabel(int entityid, string label)
        {
            ListEntities[entityid].SetData("text", label);
            return entityid;
        }

        public int DestroyEntityLabel(int entityid)
        {
            AltNetworking.RemoveEntity(ListEntities[entityid]);
            ListEntities[entityid] = null;
            return 0;
        }
        public Marker AddEntityMarker(Data.MarkerType type, Vector3 pos, Vector3 scale, int r = 225, int g = 225, int b = 225, int a = 255, int dimension = GameMode.GlobalDimension)
        {
            var data = new Marker(type, scale, r,g,b,a, this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), dimension, GameMode.Instance.StreamDistance, data.export());
            ListEntities.TryAdd(EntityNumber, item);
            return data;
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

        public async Task LoadStreamPlayer(IPlayer player)
        {
            foreach(KeyValuePair<int, object> item in ListStaticEntities)
                await AltAsync.EmitAsync(player, "createStaticEntity", item.Value);
        }
    }
}
