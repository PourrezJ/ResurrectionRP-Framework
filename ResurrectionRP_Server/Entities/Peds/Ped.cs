using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;

namespace ResurrectionRP_Server.Entities.Peds
{
    public class Ped
    {
        public static List<Ped> NPCList = new List<Ped>();

        public delegate Task NpcPrimaryCallBackAsync(IPlayer client, Ped npc);
        public delegate Task NpcSecondaryCallBackAsync(IPlayer client, Ped npc);

        public delegate void NpcPrimaryCallBack(IPlayer client, Ped npc);
        public delegate void NpcSecondaryCallBack(IPlayer client, Ped npc);

        [JsonIgnore, BsonIgnore]
        public NpcPrimaryCallBackAsync NpcInteractCallBackAsync { get; set; }
        [JsonIgnore, BsonIgnore]
        public NpcSecondaryCallBackAsync NpcSecInteractCallBackAsync { get; set; }

        [JsonIgnore, BsonIgnore]
        public NpcPrimaryCallBack NpcInteractCallBack { get; set; }
        [JsonIgnore, BsonIgnore]
        public NpcSecondaryCallBack NpcSecInteractCallBack { get; set; }

        public int ID { get; private set; }
        public PedModel Model;
        public short Dimension;
        public Vector3 Position;
        public float Rotation;
        [JsonIgnore]
        public IPlayer Owner;

        [JsonIgnore]
        public Dictionary<dynamic, dynamic> Variable = new Dictionary<dynamic, dynamic>();

        public static Ped CreateNPC(PedModel pedHash, Vector3 startPosition, float facingAngle, short dimension = GameMode.GlobalDimension)
        {
            var ped = new Ped()
            {
                ID = Streamer.Streamer.EntityNumber++,
                Model = pedHash,
                Position = startPosition,
                Rotation = facingAngle,
                Dimension = dimension
            };

            Streamer.Streamer.AddEntityPed(ped);

            NPCList.Add(ped);
            return ped;
        }

        public Dictionary<string, object> Export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = (uint)this.Model;
            data["heading"] = this.Rotation;
            data["entityType"] = (int)Streamer.Data.EntityType.Ped;
            data["id"] = this.ID;
            return data;
        }

        public static Ped GetNPCbyID(int id)
        {
            return NPCList.Find(x => x.ID == id) ?? null;
        }
    }
}
