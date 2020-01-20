using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.NetworkingEntity.Elements.Entities;
using AltV.Net.NetworkingEntity;
using AltV.Net.Data;

namespace ResurrectionRP_Server.Entities.Peds
{
    public struct WalkToData
    {
        public int Speed;
        public Vector3 Pos;

        public WalkToData(int speed, Vector3 pos)
        {
            Speed = speed;
            Pos = pos;
        }
    }

    public class Ped : Entity
    {
        public static List<Ped> NPCList = new List<Ped>();

        public delegate void NpcPrimaryCallBack(IPlayer client, Ped npc);
        public delegate void NpcSecondaryCallBack(IPlayer client, Ped npc);

        [JsonIgnore, BsonIgnore]
        public NpcPrimaryCallBack NpcInteractCallBack { get; set; }
        [JsonIgnore, BsonIgnore]
        public NpcSecondaryCallBack NpcSecInteractCallBack { get; set; }
         
        public PedModel Model;

        public float Heading;

        private IPlayer owner;
        public IPlayer Owner
        {
            get => owner;
            set
            {
                owner = value;
                if (NetworkEntity != null && NetworkEntity.Exists)
                    NetworkEntity.SetData("ownerid", ((value != null) ? Owner.Id : -1));
            }
        }

        public Ped(PedModel model, Position position, float heading, IPlayer owner = null, int dimension = GameMode.GlobalDimension) : base(position, dimension)
        {
            Model = model;
            Heading = heading;
            Owner = owner;
            NetworkEntity = AltNetworking.CreateEntity(position.ConvertToEntityPosition(), dimension, GameMode.StreamDistance, Export(), StreamingType.EntityStreaming);
        }

        public static Ped CreateNPC(PedModel pedHash, Position startPosition, float facingAngle, short dimension = GameMode.GlobalDimension, IPlayer owner = null)
        {
            var ped = new Ped(pedHash, startPosition, facingAngle, owner);

            NPCList.Add(ped);
            return ped;
        }

        public override Dictionary<string, object> Export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = (uint)Model;
            data["heading"] = Heading;
            data["entityType"] = (int)Streamer.Data.EntityType.Ped;
            data["ownerid"] = (Owner == null) ? -1 : Owner.Id;
            data["taskWanderStandard"] = false;
            return data;
        }

        public void TaskWanderStandard(bool free)
        {
            if (NetworkEntity != null && NetworkEntity.Exists)
                NetworkEntity.SetData("taskWanderStandard", free);
        }

        public void WalkTo(Vector3 pos)
        {
            if (NetworkEntity != null && NetworkEntity.Exists)
                NetworkEntity.SetData("WalkTo", new WalkToData(1, pos));
        }

        public static Ped GetNPCbyID(ulong id) => NPCList.Find(x => x.ID == id) ?? null;
    }
}
