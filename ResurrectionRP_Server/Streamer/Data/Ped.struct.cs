using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;

namespace ResurrectionRP_Server.Streamer.Data
{
    public struct Ped
    {
        public int id;
        public uint model;
        public PedType type;
        public float heading;
        public bool Freeze;
        public bool Invicible;
        public string callbackName;
        public Ped(string model, PedType type, float heading, int entityId, string callbackName = null, bool freeze = true, bool invicible = true)
        {
            this.model = Alt.Hash(model);
            this.type = type;
            this.heading = heading;
            this.Freeze = freeze;
            this.Invicible = invicible;
            this.callbackName = callbackName;
            this.id = entityId;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = this.model;
            data["type"] = (int)this.type;
            data["heading"] = this.heading;
            data["freeze"] = this.Freeze;
            data["callbackName"] = this.callbackName;
            data["invicible"] = this.Invicible;
            data["entityType"] = (int)EntityType.Ped;
            data["id"] = this.id;
            return data;
        }
    }
}
