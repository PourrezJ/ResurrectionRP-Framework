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
using Object = ResurrectionRP_Server.Entities.Objects.Object;
using TextLabel = ResurrectionRP_Server.Streamer.Data.TextLabel;
using Blips = ResurrectionRP_Server.Entities.Blips.Blips;
using AltV.Net.Data;

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

        public Object AddEntityObject(Object data)
        {
            INetworkingEntity item = AltNetworking.CreateEntity(data.position.ConvertToEntityPosition(), GameMode.GlobalDimension, GameMode.Instance.StreamDistance, data.export());
            ListEntities.TryAdd(EntityNumber, item);
            return data;
        }

        public Object UpdateEntityObject(Object obj)
        {
            INetworkingEntity oitem = this.ListEntities[obj.id];
            if (oitem.GetData("freeze", out bool freeze) && freeze != obj.freeze)
                oitem.SetData("freeze", obj.freeze);
            if (oitem.GetData("position", out string position) == true&& JsonConvert.DeserializeObject<Position>(position) != obj.position)
                oitem.Position = obj.position.ConvertToEntityPosition();
            if (oitem.GetData("rotation", out string rotation) == true)
                oitem.SetData("rotation" , JsonConvert.SerializeObject(obj.rotation));

            return obj;
        }

        public void DeleteEntityObject(Object data)
        {
            AltNetworking.RemoveEntity(ListEntities[data.id]);
            ListEntities[data.id] = null;
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

        public int DestroyEntity(int entityid)
        {
            AltNetworking.RemoveEntity(ListEntities[entityid]);
            ListEntities[entityid] = null;
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

        public async Task LoadStreamPlayer(IPlayer player)
        {
            foreach(KeyValuePair<int, object> item in ListStaticEntities)
                await AltAsync.EmitAsync(player, "createStaticEntity", item.Value);
        }
    }
}
