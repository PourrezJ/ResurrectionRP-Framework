using AltV.Net;
using AltV.Net.Async;
using System.Collections.Generic;
using System;
using ResurrectionRP_Server.Streamer.Data;
using System.Numerics;
using Newtonsoft.Json;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;

namespace ResurrectionRP_Server.Entities.Objects
{

    public class Object
    {
        public int id;
        public uint model;
        public Position position;
        public Rotation rotation;
        public bool freeze;
        public Models.Attachment attach = null;

        public Object(string model, Position position, Rotation rotation, int entityId, bool freeze = false)
        {
            this.model = Alt.Hash(model);
            this.id = entityId;
            this.freeze = freeze;
        }

        public bool SetAttach(IEntity target, string bone, Position positionOffset, Rotation rotationOffset)
        {
            //ObjectManager.AttachEntity();
            return false;
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
            data["attachment"] = this.attach;
            return data;
        }

    }
}
