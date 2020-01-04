using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Numerics;
using Newtonsoft.Json;
using WorldObject = ResurrectionRP_Server.Entities.Objects.WorldObject;
using TextLabel = ResurrectionRP_Server.Streamer.Data.TextLabel;
using Blips = ResurrectionRP_Server.Entities.Blips.Blips;
using ResurrectionRP_Server.Entities.Peds;

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
            AltNetworking.Configure(options =>
            {
                if (!string.IsNullOrEmpty(Config.GetSetting<string>("StreamerIP")))
                    options.Ip = Config.GetSetting<string>("StreamerIP");

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
            Ped ped = Ped.GetNPCbyID((int)entity.Id);
            if (ped != null)
            {
                if (ped.Owner != null) ped.Owner = null;
            }
        }

        public static int AddEntityPed(Ped ped, int dimension = GameMode.GlobalDimension)
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

        private static void OwnPed(IPlayer client, int id)
        {
            Ped ped = Ped.GetNPCbyID(id);

            if (ped != null) 
                ped.Owner = client;

            if (ListEntities.ContainsKey(id))
            {
            }
        }

        public static IPlayer TokenToPlayer(string token)
        {
            var players = Alt.GetAllPlayers();

            lock (players)
            {
                foreach (var player in players)
                {
                    if (player.TryGetNetworkingClient(out INetworkingClient netclient) && netclient.Token == token)
                        return player;
                }
            }
            return null;
        }
    }
}
