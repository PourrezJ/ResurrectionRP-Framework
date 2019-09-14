using AltV.Net;
using AltV.Net.Async;
using System.Collections.Generic;
using System;
using ResurrectionRP_Server.Streamer.Data;
using System.Numerics;
using Newtonsoft.Json;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using AltV.Net.NetworkingEntity.Elements.Entities;

namespace ResurrectionRP_Server.Entities.Objects
{

    public class Object
    {
        public int id;
        public string model;
        public Position position;
        public Rotation rotation;
        public uint dimension;
        public bool freeze;
        public Models.Attachment attach = null;
        public string pickup = null;


        public Object(string model, Position position, Rotation rotation, int entityId, bool freeze = false, uint dimension = ushort.MaxValue, string pickup = null)
        {
            this.model = model;
            this.id = entityId;
            this.freeze = freeze;
            this.position = position;
            this.rotation = rotation;
            this.pickup = pickup;
        }

        public bool SetAttachToEntity(IEntity target, string bone, Position positionOffset, Rotation rotationOffset)
        {
            ObjectManager.AttachToEntity(target, this, bone, positionOffset, rotationOffset);
            return true;
        }

        public bool DetachAttach()
        {
            this.attach = null;
            return true;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = this.model;
            data["entityType"] = (int)EntityType.Object;
            data["id"] = this.id;
            data["position"] = JsonConvert.SerializeObject(this.id);
            data["freeze"] = this.freeze;
            data["pickup"] = this.pickup;
            data["attachment"] = JsonConvert.SerializeObject(this.attach);
            return data;
        }

    }
}
