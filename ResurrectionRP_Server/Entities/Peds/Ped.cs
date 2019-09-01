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

        public delegate Task NpcPrimaryCallBack(IPlayer client, Ped npc);
        public delegate Task NpcSecondaryCallBack(IPlayer client, Ped npc);

        [JsonIgnore, BsonIgnore]
        public NpcPrimaryCallBack NpcInteractCallBack { get; set; }
        [JsonIgnore, BsonIgnore]
        public NpcSecondaryCallBack NpcSecInteractCallBack { get; set; }

        public int ID { get; private set; }
        public PedModel Model;
        public Streamer.Data.PedType PedType;
        public uint Dimension;
        public Vector3 Position;
        public float Rotation;
        public bool Freeze;

        public uint VehicleID = uint.MaxValue;
        public bool IsInVehicle
        {
            get => VehicleID != uint.MaxValue;
        }

        public bool Visible;
        public bool CanRagdoll;
        public bool Invincible;
        public IPlayer Owner;
        public float Speed;
        public int Seat;
        [JsonIgnore]
        public Dictionary<dynamic, dynamic> Variable = new Dictionary<dynamic, dynamic>();

        public static async Task<Ped> CreateNPC(PedModel pedHash, Streamer.Data.PedType pedType,Vector3 startPosition, float facingAngle, uint dimension = (uint)ushort.MaxValue, bool freeze = true, bool visible = true, bool invincible = true, bool canRagdoll = false)
        {
            var ped = new Ped()
            {
                ID = GameMode.Instance.Streamer.EntityNumber++,
                Model = pedHash,
                Position = startPosition,
                Rotation = facingAngle,
                Freeze = freeze,
                Visible = visible,
                Dimension = dimension,
                CanRagdoll = canRagdoll,
                PedType = pedType
            };

            GameMode.Instance.Streamer.AddEntityPed(ped);

            PedsManager.NPCList.Add(ped);
            return ped;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = (uint)this.Model;
            data["type"] = (int)this.PedType;
            data["heading"] = this.Rotation;
            data["freeze"] = this.Freeze;
            data["invicible"] = this.Invincible;
            data["entityType"] = (int)Streamer.Data.EntityType.Ped;
            data["id"] = this.ID;
            return data;
        }
    }
}
