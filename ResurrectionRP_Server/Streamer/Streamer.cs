using AltV.Net.Data;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using Ped = ResurrectionRP_Server.Streamer.Data.Ped;
using Object = ResurrectionRP_Server.Streamer.Data.Object;
using TextLabel = ResurrectionRP_Server.Streamer.Data.TextLabel;
using PedType = ResurrectionRP_Server.Streamer.Data.PedType;
using Marker = ResurrectionRP_Server.Streamer.Data.Marker;
using Blips = ResurrectionRP_Server.Streamer.Data.Blips;

namespace ResurrectionRP_Server.Streamer
{
    public partial class Streamer
    {
        public int EntityNumber = 0;
        public Dictionary<int,INetworkingEntity> ListEntities = new Dictionary<int, INetworkingEntity>();

        public int StaticEntityNumber = 0;
        public Dictionary<int, object> ListStaticEntities = new Dictionary<int, object>();

        public Streamer()
        {
            //var data = new Marker(Data.MarkerType.HorizontalSplitArrowCircle, new Vector3(1, 1, 1), 255, 0, 0, 255, this.EntityNumber++);
            try
            {
                AltNetworking.Configure(options =>
                {
                    options.Port = 46429;
                });

                AltNetworking.OnEntityStreamIn = (entity, client) =>
                {
                    if(GameMode.Instance.IsDebug)
                        Console.WriteLine("streamed in " + entity.Id + " in client " + client.Token);
                };

                AltNetworking.OnEntityStreamOut = (entity, client) =>
                {
                    if (GameMode.Instance.IsDebug)
                        Console.WriteLine("streamed out " + entity.Id + " in client " + client.Token);
                };
            }
            catch(Exception ex)
            {
                AltV.Net.Alt.Server.LogError(ex.ToString());
            }
            //addEntityMarker(Data.MarkerType.VerticalCylinder, new Vector3(404,-991,30), new Vector3(2,2,2));



        }

        public int addEntityPed(PedType type, string model, Vector3 pos, float heading)
        {
            var data = new Ped(model, type, heading, this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, data.export());
            this.ListEntities.Add(this.EntityNumber, item);
            return this.EntityNumber;
        }
        public int addEntityObject(string model, Vector3 pos)
        {
            var data = new Object(model,  this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, data.export());
            this.ListEntities.Add(this.EntityNumber, item);
            return this.EntityNumber;
        }
        public int addEntityTextLabel(string label, Vector3 pos, int font = 1, int r = 255, int g = 255, int b = 255, int a = 255)
        {
            var data = new TextLabel(label, font, r, g, b, a, this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance / 25, data.export());
            this.ListEntities.Add(this.EntityNumber, item);
            return this.EntityNumber;
        }
        public int addEntityMarker(Data.MarkerType type, Vector3 pos, Vector3 scale, int r = 225, int g = 225, int b = 225, int a = 255)
        {
            var data = new Marker(type, scale, r,g,b,a, this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, data.export());
            this.ListEntities.Add(this.EntityNumber, item);
            return this.EntityNumber;
        }

        public int addStaticEntityBlip(string name, Vector3 pos, int color, int sprite)
        {
            this.ListStaticEntities.Add(this.StaticEntityNumber, new Blips(name, pos,color, sprite, this.StaticEntityNumber++).export());
            return this.StaticEntityNumber;
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
