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
                AltV.Net.Alt.Server.LogError(ex.ToString());
            }

        }

        public int addEntityPed(Entities.Peds.Ped ped)
        {
            INetworkingEntity item = AltNetworking.CreateEntity(ped.Position.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, ped.export());
            this.ListEntities.TryAdd(this.EntityNumber, item);
            return this.EntityNumber;
        }
        public int addEntityObject(string model, Vector3 pos)
        {
            var data = new Object(model,  this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, data.export());
            this.ListEntities.TryAdd(this.EntityNumber, item);
            return this.EntityNumber;
        }
        public int addEntityTextLabel(string label, Vector3 pos, int font = 1, int r = 255, int g = 255, int b = 255, int a = 255)
        {
            var data = new TextLabel(label, font, r, g, b, a, this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance / 25, data.export());
            this.ListEntities.TryAdd(this.EntityNumber, item);
            return this.EntityNumber;
        }

        public int updateEntityTextLabel(int entityid, string label)
        {
            ListEntities[entityid].SetData("text", label);
            return entityid;
        }

        public int destroyEntityLabel(int entityid)
        {
            AltNetworking.RemoveEntity(ListEntities[entityid]);
            ListEntities[entityid] = null;
            return 0;
        }
        public int addEntityMarker(Data.MarkerType type, Vector3 pos, Vector3 scale, int r = 225, int g = 225, int b = 225, int a = 255)
        {
            var data = new Marker(type, scale, r,g,b,a, this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, data.export());
            this.ListEntities.TryAdd(this.EntityNumber, item);
            return this.EntityNumber;
        }

        public int addStaticEntityBlip(Blips blip)
        {
            this.ListStaticEntities.TryAdd(blip.id, blip.export());
            if (GameMode.Instance.PlayerList.Count > 0)
                AltAsync.EmitAllClients("createStaticEntity", ListStaticEntities[blip.id]);
            return blip.id;
        }

        public async Task updateStaticEntityBlip( Blips blip)
        {
            this.ListStaticEntities[blip.id] = blip;
            GameMode.Instance.PlayerList.ForEach(async (player) =>
            {
                await AltAsync.EmitAsync(player, "deleteStaticEntity", blip.id, (int)blip.type);
                await AltAsync.EmitAsync(player, "createStaticEntity", blip.export());
            });
        }

        public async Task destroyStaticEntityBlip(Blips blip)
        {
            GameMode.Instance.PlayerList.ForEach(async (player) =>
            {
                await AltAsync.EmitAsync(player, "deleteStaticEntity", blip.id, (int)blip.type);
            });
            this.ListStaticEntities.TryRemove(blip.id, out dynamic bliped);
        }

        public async Task LoadStreamPlayer(IPlayer player)
        {
            foreach(KeyValuePair<int, object> item in ListStaticEntities)
            {
                await AltAsync.EmitAsync(player, "createStaticEntity", item.Value);
            }
        }

    }
}
