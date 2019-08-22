using AltV.Net.Data;
using AltV.Net;
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

namespace ResurrectionRP_Server.Streamer
{
    public class Streamer
    {
        public int EntityNumber = 0;
        public Dictionary<int,INetworkingEntity> ListEntities = new Dictionary<int, INetworkingEntity>();

        public Streamer()
        {
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
        public int addEntityTextLabel(string label, Vector3 pos, int font = 1, int r = 225, int g = 225, int b = 225, int a = 100)
        {
            var data = new TextLabel(label, font, r, g, b, a, this.EntityNumber++);
            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance / 3, data.export());
            this.ListEntities.Add(this.EntityNumber, item);
            return this.EntityNumber;
        }

    }
}
