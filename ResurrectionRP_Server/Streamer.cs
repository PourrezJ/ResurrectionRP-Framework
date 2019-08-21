using AltV.Net.Data;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server
{
    public class Streamer
    {
        public Streamer()
        {
            try
            {
                AltNetworking.Configure(options =>
                {
                    options.Port = 46429;
                });

                var data = new Dictionary<string, object>(); // This is the entity data, the streamer on clientside will receive it with entity.data
                data["model"] = "a_c_deer";
                var test = AltNetworking.CreateEntity(new Entity.Position { X = 0, Y = 0, Z = 73 }, GameMode.GlobalDimension, 50, data, StreamingType.EntityStreaming);

                AltNetworking.OnEntityStreamIn = (entity, client) =>
                {
                    Console.WriteLine("streamed in " + entity.Id + " in client " + client.Token);
                };

                AltNetworking.OnEntityStreamOut = (entity, client) =>
                {
                    Console.WriteLine("streamed out " + entity.Id + " in client " + client.Token);
                };
            }
            catch(Exception ex)
            {
                AltV.Net.Alt.Server.LogError(ex.ToString());
            }
        }
    }
}
